using ASC.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ASC.Utilities;

namespace ASC.Model.Queries
{
    public static class Queries
    {
        public static Expression<Func<ServiceRequest, bool>> GetDashboardQuery(DateTime? requestedDate,
            List<string> status = null,
            string email = "",
            string serviceEngineerEmail = "")
        {
            var query = (Expression<Func<ServiceRequest, bool>>)(u => true);

            // Add Requested Date Clause
            if (requestedDate.HasValue)
            {
                var requestedDateFilter = (Expression<Func<ServiceRequest, bool>>)(u => u.RequestedDate >= requestedDate);
                query = query.And(requestedDateFilter);
            }

            // Add Email clause if email is passed as a parameter
            if (!string.IsNullOrWhiteSpace(email))
            {
                // Assuming the intention was to filter by email here, not reuse requestedDateFilter
                var emailFilter = (Expression<Func<ServiceRequest, bool>>)(u => u.PartitionKey == email);
                query = query.And(emailFilter); // Corrected to use emailFilter
            }

            // Add Service Engineer Email clause if email is passed as a parameter
            if (!string.IsNullOrWhiteSpace(serviceEngineerEmail))
            {
                // Assuming the intention was to filter by serviceEngineerEmail here, not reuse requestedDateFilter
                var serviceEngineerEmailFilter = (Expression<Func<ServiceRequest, bool>>)(u => u.ServiceEngineer == serviceEngineerEmail);
                query = query.And(serviceEngineerEmailFilter); // Corrected to use serviceEngineerEmailFilter
            }

            // Add Status clause if status is passed a parameter.
            // Individual status clauses are appended with OR condition
            var statusQueries = (Expression<Func<ServiceRequest, bool>>)(u => false);
            if (status != null)
            {
                foreach (var state in status)
                {
                    var statusFilter = (Expression<Func<ServiceRequest, bool>>)(u => u.Status == state);
                    statusQueries = statusQueries.Or(statusFilter);
                }
                query = query.And(statusQueries);
            }
            return query;
        }
    }
}
