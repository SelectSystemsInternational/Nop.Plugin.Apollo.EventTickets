using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;

using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;

using Nop.Plugin.Apollo.EventTickets.Settings;

namespace Nop.Plugin.Apollo.EventTickets.Services;

/// <summary>
/// Workflow message service
/// </summary>
public partial class EventTicketsMessageService : IEventTicketsMessageService
{

    #region Fields

    protected readonly EmailAccountSettings _emailAccountSettings;
    protected readonly IAddressService _addressService;
    protected readonly IEmailAccountService _emailAccountService;
    protected readonly IEventPublisher _eventPublisher;
    protected readonly ILanguageService _languageService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IMessageTemplateService _messageTemplateService;
    protected readonly IMessageTokenProvider _messageTokenProvider;
    protected readonly IOrderService _orderService;
    protected readonly IQueuedEmailService _queuedEmailService;
    protected readonly IStoreContext _storeContext;
    protected readonly IStoreService _storeService;
    protected readonly ITokenizer _tokenizer;
    protected readonly INopFileProvider _fileProvider;
    protected readonly ISettingService _settingService;
    protected readonly IProductService _productService;
    protected readonly IProductAttributeService _productAttributeService;

    #endregion

    #region Ctor

    public EventTicketsMessageService(EmailAccountSettings emailAccountSettings,
        IAddressService addressService,
        IEmailAccountService emailAccountService,
        IEventPublisher eventPublisher,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IMessageTemplateService messageTemplateService,
        IMessageTokenProvider messageTokenProvider,
        IOrderService orderService,
        IQueuedEmailService queuedEmailService,
        IStoreContext storeContext,
        IStoreService storeService,
        ITokenizer tokenizer, 
        INopFileProvider fileProvider,
        ISettingService settingService,
        IProductService productService,
        IProductAttributeService productAttributeService)
    {
        _emailAccountSettings = emailAccountSettings;
        _addressService = addressService;
        _emailAccountService = emailAccountService;
        _eventPublisher = eventPublisher;
        _languageService = languageService;
        _localizationService = localizationService;
        _messageTemplateService = messageTemplateService;
        _messageTokenProvider = messageTokenProvider;
        _orderService = orderService;
        _queuedEmailService = queuedEmailService;
        _storeContext = storeContext;
        _storeService = storeService;
        _tokenizer = tokenizer;
        _fileProvider = fileProvider;
        _settingService = settingService;
        _productService = productService;
        _productAttributeService = productAttributeService;
    }

    #endregion

    #region Utilities

    public async Task<string> GetProductEmailAttachementAsync(int productId)
    {
        string file = null;
        var product = await _productService.GetProductByIdAsync(productId);
        if (product != null)
        {
            var documentLink = (await _productAttributeService.GetAllProductAttributesAsync()).Where(name => name.Name.Equals("Document Link")).FirstOrDefault();
            var parentProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
            if (parentProduct != null && documentLink != null)
            {
                var productAttributMapping = (await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(parentProduct.Id))
                    .Where(attr => attr.ProductAttributeId == documentLink.Id);
                if (productAttributMapping != null && productAttributMapping.Count() > 0)
                    file = productAttributMapping.FirstOrDefault().DefaultValue;
            }
        }

        return file;

    }

    /// <summary>
    /// Ensure language is active
    /// </summary>
    /// <param name="languageId">Language identifier</param>
    /// <param name="storeId">Store identifier</param>
    /// <returns>Return a value language identifier</returns>
    protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
    {
        //load language by specified ID
        var language = await _languageService.GetLanguageByIdAsync(languageId);

        if (language == null || !language.Published)
        {
            //load any language from the specified store
            language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
        }

        if (language == null || !language.Published)
        {
            //load any language
            language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
        }

        if (language == null)
            throw new Exception("No active language could be loaded");

        return language.Id;
    }

    /// <summary>
    /// Get active message templates by the name
    /// </summary>
    /// <param name="messageTemplateName">Message template name</param>
    /// <param name="storeId">Store identifier</param>
    /// <returns>List of message templates</returns>
    protected virtual async Task<IList<MessageTemplate>> GetActiveMessageTemplates(string messageTemplateName, int storeId)
    {
        //get message templates by the name
        var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(messageTemplateName, storeId);

        //no template found
        if (!messageTemplates?.Any() ?? true)
            return new List<MessageTemplate>();

        //filter active templates
        messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

        return messageTemplates;
    }

    /// <summary>
    /// Get EmailAccount to use with a message templates
    /// </summary>
    /// <param name="messageTemplate">Message template</param>
    /// <param name="languageId">Language identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the emailAccount
    protected virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
    {
        var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.EmailAccountId, languageId);
        //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
        if (emailAccountId == 0)
            emailAccountId = messageTemplate.EmailAccountId;

        var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId) ?? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                           (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
        return emailAccount;
    }

    /// <summary>
    /// Send notification
    /// </summary>
    /// <param name="messageTemplate">Message template</param>
    /// <param name="emailAccount">Email account</param>
    /// <param name="languageId">Language identifier</param>
    /// <param name="tokens">Tokens</param>
    /// <param name="toEmailAddress">Recipient email address</param>
    /// <param name="toName">Recipient name</param>
    /// <param name="attachmentFilePath">Attachment file path</param>
    /// <param name="attachmentFileName">Attachment file name</param>
    /// <param name="replyToEmailAddress">"Reply to" email</param>
    /// <param name="replyToName">"Reply to" name</param>
    /// <param name="fromEmail">Sender email. If specified, then it overrides passed "emailAccount" details</param>
    /// <param name="fromName">Sender name. If specified, then it overrides passed "emailAccount" details</param>
    /// <param name="subject">Subject. If specified, then it overrides subject of a message template</param>
    /// <returns>Queued email identifier</returns>
    public virtual async Task<int> SendNotificationAsync(MessageTemplate messageTemplate,
        EmailAccount emailAccount, int languageId, IEnumerable<Token> tokens,
        string toEmailAddress, string toName,
        string attachmentFilePath = null, string attachmentFileName = null,
        string replyToEmailAddress = null, string replyToName = null,
        string fromEmail = null, string fromName = null, string subject = null)
    {
        if (messageTemplate == null)
            throw new ArgumentNullException(nameof(messageTemplate));

        if (emailAccount == null)
            throw new ArgumentNullException(nameof(emailAccount));

        //retrieve localized message template data
        var bcc = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.BccEmailAddresses, languageId);
        if (string.IsNullOrEmpty(subject))
            subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject, languageId);
        var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId);

        //Replace subject and body tokens 
        var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
        var bodyReplaced = _tokenizer.Replace(body, tokens, true);

        //limit name length
        toName = CommonHelper.EnsureMaximumLength(toName, 300);

        var email = new QueuedEmail
        {
            Priority = QueuedEmailPriority.High,
            From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
            FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
            To = toEmailAddress,
            ToName = toName,
            ReplyTo = replyToEmailAddress,
            ReplyToName = replyToName,
            CC = string.Empty,
            Bcc = bcc,
            Subject = subjectReplaced,
            Body = bodyReplaced,
            AttachmentFilePath = attachmentFilePath,
            AttachmentFileName = attachmentFileName,
            AttachedDownloadId = messageTemplate.AttachedDownloadId,
            CreatedOnUtc = DateTime.UtcNow,
            EmailAccountId = emailAccount.Id,
            DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                : (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
        };

        await _queuedEmailService.InsertQueuedEmailAsync(email);
        return email.Id;
    }


    #endregion

    #region Methods

    #endregion

    #region Orders

    /// <summary>
    /// Sends an order placed notification to a customer
    /// </summary>
    /// <param name="order">Order instance</param>
    /// <param name="languageId">Message language identifier</param>
    /// <param name="attachmentFilePath">Attachment file path</param>
    /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
    /// <returns>Queued email identifier</returns>
    public virtual async Task<IList<int>> SendOrderApprovedCustomerNotificationAsync(Order order, int languageId,
        string attachmentFilePath = null, string attachmentFileName = null)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        //Get settings for order
        var eventTicketSettings = await _settingService.LoadSettingAsync<EventTicketSettings>(store.Id);

        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
        foreach (var oi in orderItems)
        {
            if (attachmentFilePath == null)
            {
                attachmentFileName = await GetProductEmailAttachementAsync(oi.ProductId);
                if (attachmentFileName != null)
                    attachmentFilePath = _fileProvider.MapPath(eventTicketSettings.AttachFileLocation) + attachmentFileName;
            }
        }

        var messageTemplates = new List<MessageTemplate>();
        messageTemplates = (await GetActiveMessageTemplates("Apollo.EventTickets.OrderApproved.CustomerNotification", store.Id)).ToList();

        if (!messageTemplates.Any())
            return new List<int>();

        //tokens
        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            var toEmail = billingAddress.Email;
            var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                attachmentFilePath, attachmentFileName);

        }).ToListAsync();
    }

    /// <summary>
    /// Sends an order completed notification to a customer
    /// </summary>
    /// <param name="order">Order instance</param>
    /// <param name="languageId">Message language identifier</param>
    /// <param name="attachmentFilePath">Attachment file path</param>
    /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
    /// <returns>Queued email identifier</returns>
    public virtual async Task<IList<int>> SendOrderFinalisedCustomerNotificationAsync(Order order, int languageId,
        string attachmentFilePath = null, string attachmentFileName = null)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        //Get settings for order
        var eventTicketSettings = await _settingService.LoadSettingAsync<EventTicketSettings>(store.Id);

        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
        foreach (var oi in orderItems)
        {
            if (attachmentFilePath == null)
            {
                attachmentFileName = await GetProductEmailAttachementAsync(oi.ProductId);
                if (attachmentFileName != null)
                    attachmentFilePath = _fileProvider.MapPath(eventTicketSettings.AttachFileLocation) + attachmentFileName;
            }
        }

        var messageTemplates = new List<MessageTemplate>();
        messageTemplates = (await GetActiveMessageTemplates("Apollo.EventTickets.OrderFinalised.CustomerNotification", store.Id)).ToList();

        if (!messageTemplates.Any())
            return new List<int>();

        //tokens
        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            var toEmail = billingAddress.Email;
            var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                attachmentFilePath, attachmentFileName);

        }).ToListAsync();
    }

    #endregion

    #region Event Tickets

    /// <summary>
    /// Sends the Sales Summary Report to vendors and Stores 
    /// </summary>
    /// <param name="store">Store instance</param>
    /// <param name="vendor">Vendor instance</param>/// 
    /// <param name="languageId">Message language identifier</param>
    /// <param name="attachmentFilePath">Attachment file path</param>
    /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
    /// <returns>Queued email identifier</returns>
    public virtual async Task<IList<int>> SendSummaryReportNotificationAsync(Store store, Vendor vendor, Affiliate affiliate, int languageId,
        string attachmentFilePath = null, string attachmentFileName = null)
    {

        string sendToEmail = string.Empty;
        string sendToName = string.Empty;

        if (store == null)
            throw new ArgumentNullException(nameof(vendor));
        else if (vendor != null)
        {
            sendToEmail = vendor.Email;
            sendToName = vendor.Name;
        }
        else if (affiliate != null)
        {
            var address = await _addressService.GetAddressByIdAsync(affiliate.AddressId);
            if (address != null)
            {
                sendToEmail = address.Email;
                sendToName = address.FirstName + " " + address.LastName;
            }
        }

        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        //Get settings for order
        var eventTicketSettings = await _settingService.LoadSettingAsync<EventTicketSettings>(store.Id);

        var messageTemplates = new List<MessageTemplate>();
        messageTemplates = (await GetActiveMessageTemplates("Apollo.EventTickets.SummaryReportNotification", store.Id)).ToList();

        if (!messageTemplates.Any())
            return new List<int>();

        //tokens
        var commonTokens = new List<Token>();
        if (vendor != null)
            await _messageTokenProvider.AddVendorTokensAsync(commonTokens, vendor);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var toEmail = sendToEmail == string.Empty ? emailAccount.Email: sendToEmail;
            var toName = sendToName == string.Empty ? emailAccount.DisplayName : sendToName;

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                attachmentFilePath, attachmentFileName);

        }).ToListAsync();
    }

    /// <summary>
    /// Sends the Sales Summary Report to vendors and Stores 
    /// </summary>
    /// <param name="store">Store instance</param>
    /// <param name="vendor">Vendor instance</param>/// 
    /// <param name="languageId">Message language identifier</param>
    /// <param name="attachmentFilePath">Attachment file path</param>
    /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
    /// <returns>Queued email identifier</returns>
    public virtual async Task<IList<int>> SendEventDoorReportNotificationAsync(Store store, Vendor vendor, Product ticket, int languageId,
        string attachmentFilePath = null, string attachmentFileName = null)
    {
        string sendToEmail = string.Empty;
        string sendToName = string.Empty;

        if (store == null)
            throw new ArgumentNullException(nameof(vendor));
        else if (vendor != null)
        {
            sendToEmail = vendor.Email;
            sendToName = vendor.Name;
        }

        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        //Get settings for order
        var eventTicketSettings = await _settingService.LoadSettingAsync<EventTicketSettings>(store.Id);

        var messageTemplates = new List<MessageTemplate>();
        messageTemplates = (await GetActiveMessageTemplates("Apollo.EventTickets.DoorReportNotification", store.Id)).ToList();

        if (!messageTemplates.Any())
            return new List<int>();

        //tokens
        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddProductTokensAsync(commonTokens, ticket, languageId);
        if (vendor != null)
            await _messageTokenProvider.AddVendorTokensAsync(commonTokens, vendor);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount, languageId);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var toEmail = sendToEmail == string.Empty ? emailAccount.Email : sendToEmail;
            var toName = sendToName == string.Empty ? emailAccount.DisplayName : sendToName;

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                attachmentFilePath, attachmentFileName);

        }).ToListAsync();
    }

    #endregion

}