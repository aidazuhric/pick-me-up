import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/order.dart';
import 'base_provider.dart';

class OrderProvider extends BaseProvider<Order> {
  OrderProvider() : super('Orders');

  @override
  Order fromJson(dynamic data) => Order.fromJson(data as Map<String, dynamic>);

  Future<Order> accept(int orderId) => _post('$orderId/Accept');

  Future<Order> reject(int orderId) => _post('$orderId/Reject');

  Future<Order> pickup(int orderId) => _post('$orderId/Pickup');

  Future<Order> complete(int orderId) => _post('$orderId/Complete');

  Future<Order> cancel(int orderId, String reason) async {
    final response = await http.post(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/$orderId/Cancel'),
      headers: createHeaders(),
      body: jsonEncode({'reason': reason}),
    );
    validateResponse(response);

    return Order.fromJson(jsonDecode(response.body));
  }

  Future<Order> _post(String path) async {
    final response = await http.post(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/$path'),
      headers: createHeaders(),
      body: jsonEncode(const <String, dynamic>{}),
    );
    validateResponse(response);

    return Order.fromJson(jsonDecode(response.body));
  }
}
