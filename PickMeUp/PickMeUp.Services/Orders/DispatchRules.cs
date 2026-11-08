using System.Linq.Expressions;
using PickMeUp.Model.Enums;
using PickMeUp.Services.Database;

namespace PickMeUp.Services.Orders
{
    public static class DispatchRules
    {
        public static readonly int[] ActiveRideStatuses =
        {
            (int)OrderStatusEnum.Confirmed,
            (int)OrderStatusEnum.InProgress
        };

        public static Expression<Func<Driver, bool>> OperationalDriver =>
            driver => driver.ResignedAt == null
                && driver.IsActive
                && driver.User.IsActive
                && driver.TaxiCompany.IsActive;

        public static bool IsOfferedTo(int? orderCompanyId, int? orderCityId, int driverCompanyId, int? driverCityId) =>
            (orderCompanyId == null || orderCompanyId == driverCompanyId)
            && (orderCityId == null || orderCityId == driverCityId);

        public static Expression<Func<Order, bool>> OfferedTo(int driverCompanyId, int? driverCityId) =>
            order => (order.TaxiCompanyId == null || order.TaxiCompanyId == driverCompanyId)
                && (order.CityId == null || order.CityId == driverCityId);

        public static Expression<Func<Driver, bool>> DriversOffered(int? orderCompanyId, int? orderCityId) =>
            driver => (orderCompanyId == null || driver.TaxiCompanyId == orderCompanyId)
                && (orderCityId == null || driver.TaxiCompany.CityId == orderCityId);

        public static Expression<Func<Order, bool>> VisibleToDriver(int driverId, int driverCompanyId, int? driverCityId) =>
            order => order.DriverId == driverId
                || (order.OrderStatusId == (int)OrderStatusEnum.Pending
                    && order.IsPrepaid
                    && (order.TaxiCompanyId == null || order.TaxiCompanyId == driverCompanyId)
                    && (order.CityId == null || order.CityId == driverCityId)
                    && !order.Rejections.Any(r => r.DriverId == driverId));

        public static Expression<Func<Order, bool>> AcceptableBy(int driverId, int driverCompanyId, int? driverCityId) =>
            order => order.OrderStatusId == (int)OrderStatusEnum.Pending
                && order.IsPrepaid
                && order.DriverId == null
                && (order.TaxiCompanyId == null || order.TaxiCompanyId == driverCompanyId)
                && (order.CityId == null || order.CityId == driverCityId)
                && !order.Rejections.Any(r => r.DriverId == driverId);

        public static bool IsAcceptableBy(Order order, int driverId, int driverCompanyId, int? driverCityId) =>
            AcceptableBy(driverId, driverCompanyId, driverCityId).Compile().Invoke(order);

        public static Expression<Func<Order, bool>> ActiveRideOf(int driverId) =>
            order => order.DriverId == driverId && ActiveRideStatuses.Contains(order.OrderStatusId);

        public static Expression<Func<CarAssignment, bool>> AssignmentActiveAt(DateTime moment) =>
            assignment => assignment.IsActive
                && assignment.AssignedFrom <= moment
                && (assignment.AssignedTo == null || assignment.AssignedTo > moment);
    }
}
