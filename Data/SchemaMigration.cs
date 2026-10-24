using Nop.Core;
using Nop.Data.Migrations;
using Nop.Services.Configuration;
using Nop.Services.Stores;

using Nop.Plugin.Apollo.EventTickets.Settings;

namespace Nop.Plugin.Apollo.EventTickets.Data
{
    [NopMigration("2022/03/21 08:40:55:1687541", "Apollo.EventTickets base schema")]
    public class SchemaMigration : FluentMigrator.Migration
    {
        protected IMigrationManager _migrationManager;
        protected readonly ISettingService _settingService;
        protected readonly IStoreService _storeService;
        protected readonly IStoreContext _storeContext;
        protected readonly EventTicketSettings _eventTicketsSettings;

        public SchemaMigration(IMigrationManager migrationManager,
            ISettingService settingService,
            IStoreService storeService,
            IStoreContext storeContext,
            EventTicketSettings eventTicketsSettings)
        {
            _migrationManager = migrationManager;
            _settingService = settingService;
            _storeService = storeService;
            _storeContext = storeContext;
            _eventTicketsSettings = eventTicketsSettings;
        }

        public override void Up()
        {
            try
            {

            }
            catch
            {
                // The Database Files may still exist if not deleted from last unistall 
            }
        }

        public override void Down()
        {

        }
    }
}