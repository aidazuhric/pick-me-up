import '../models/order.dart';
import 'base_provider.dart';

class OrderProvider extends BaseProvider<Order> {
  OrderProvider() : super('Orders');

  @override
  Order fromJson(dynamic data) => Order.fromJson(data as Map<String, dynamic>);
}
