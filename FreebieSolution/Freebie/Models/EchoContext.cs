using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Infrastructure;

namespace Freebie.Models
{
	public class EchoContext : DbContext
	{

		public DbSet<Account> Accounts { get; set; }
		public DbSet<Gender> Genders { get; set; }
		public DbSet<Status> Statuses { get; set; }
		public DbSet<IncomeRange> IncomeRanges { get; set; }
		public DbSet<Occupation> Occupations { get; set; }
		public DbSet<Education> Educations { get; set; }
		public DbSet<Quota> Quotas { get; set; }
		public DbSet<Channel> Channels { get; set; }
		public DbSet<MaritalStatus> MaritalStatuses { get; set; }
		public DbSet<CodeControl> CodeControls { get; set; }
		public DbSet<AccountMobile> AccountMobiles { get; set; }
		public DbSet<Interest> Interests { get; set; }
		public DbSet<AccountInterest> AccountInterests { get; set; }
        public DbSet<AccountActivation> AccountActivations { get; set; }
        public DbSet<AccountTrial> AccountTrials { get; set; }
        public DbSet<AdminConfiguration> AdminConfigurations { get; set; }

        public DbSet<AccountQuota> AccountQuotas { get; set; }
        public DbSet<OTPRequest> OTPRequests { get; set; }
        public DbSet<OTP> OTPs { get; set; }

        public DbSet<PageMap> PageMaps { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<EventAction> EventActions { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<AccountQuotaUsedCur> AccountQuotaUsedCurs { get; set; }
        //backend
        public DbSet<Role> Roles { get; set; }
        public DbSet<Dept> Depts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<EmailRequest> EmailRequests { get; set; }

        public DbSet<SmsRegistrationLog> SmsRegistrationLogs { get; set; }
        public DbSet<OtpLog> OtpLogs { get; set; }
        public DbSet<ActivationSmsLog> ActivationSmsLogs { get; set; }
        public DbSet<Zipcode> Zipcodes { get; set; }
        public DbSet<PersonalIncomeRange> PersonalIncomeRanges { get; set; }
		public EchoContext(): base() {

		}
		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
			Database.SetInitializer<EchoContext>(null);
		}

        public override int SaveChanges()
        {
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;


            return base.SaveChanges();
        }
	}
}