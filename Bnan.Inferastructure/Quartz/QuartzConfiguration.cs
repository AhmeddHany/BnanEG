using Quartz;

namespace Bnan.Inferastructure.Quartz
{
    public class QuartzConfiguration
    {
        public static void ConfigureQuartz(IServiceCollectionQuartzConfigurator quartzConfigurator)
        {
            //// Add UpdateContract job
            //var updateJobKey = new JobKey("UpdateContractJob");
            //quartzConfigurator.AddJob<UpdateContractJob>(opts => opts.WithIdentity(updateJobKey));

            //// Trigger for UpdateContractJob, configure as needed
            //quartzConfigurator.AddTrigger(opts => opts
            //    .ForJob(updateJobKey)
            //    .WithIdentity("UpdateContractJob-trigger")
            //    .StartNow()  // Example: Start immediately
            //    .WithSimpleSchedule(x => x
            //        .WithIntervalInMinutes(10)  // Repeat every 10 minutes
            //        .RepeatForever()));         // Repeat indefinitely or adjust as needed


            ////Add RefreshLogin job
            //var refreshJobKey = new JobKey("RefreshLoginJob");
            //quartzConfigurator.AddJob<RefreshLoginJob>(opts => opts.WithIdentity(refreshJobKey));
            //quartzConfigurator.AddTrigger(opts => opts
            //    .ForJob(refreshJobKey)
            //    .WithIdentity("RefreshLoginJob-trigger")
            //    .StartNow()
            //    .WithSimpleSchedule(x => x
            //        .WithIntervalInMinutes(10)
            //        .RepeatForever()));

            // Add UpdateStatusForDocsAndPriceCar job
            var updateStatusJobKey = new JobKey("UpdateStatusForDocsAndPriceCarJob");
            quartzConfigurator.AddJob<UpdateStatusForDocsAndPriceCarJob>(opts => opts.WithIdentity(updateStatusJobKey));
            quartzConfigurator.AddTrigger(opts => opts
                .ForJob(updateStatusJobKey)
                .WithIdentity("UpdateStatusForDocsAndPriceCarJob-trigger")
                .WithCronSchedule("0 1 0 * * ?")); // Runs daily at 12:01 AM

            // Add UpdateCounterForSomeTables job
            var updateCounterJobKey = new JobKey("UpdateCounterForSomeTablesJob");
            quartzConfigurator.AddJob<UpdateCounterForSomeTables>(opts => opts.WithIdentity(updateCounterJobKey));
            quartzConfigurator.AddTrigger(opts => opts
                .ForJob(updateCounterJobKey)
                .WithIdentity("UpdateCounterForSomeTables-trigger")
                .WithCronSchedule("0 0 1 * * ?")); // Runs daily at 1:00 AM
        }
    }
}
