import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/payable_order.dart';
import '../models/payment.dart';
import 'base_provider.dart';

class PaymentProvider extends BaseProvider<Payment> {
  PaymentProvider() : super('Payments');

  @override
  Payment fromJson(dynamic data) =>
      Payment.fromJson(data as Map<String, dynamic>);

  Future<PaymentConfig> readConfig() async {
    final response = await http.get(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/Config'),
      headers: createHeaders(),
    );
    validateResponse(response);

    return PaymentConfig.fromJson(jsonDecode(response.body));
  }

  Future<List<PayableOrder>> readPayable() async {
    final response = await http.get(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/Payable'),
      headers: createHeaders(),
    );
    validateResponse(response);

    final decoded = jsonDecode(response.body) as List<dynamic>;

    return decoded
        .map((e) => PayableOrder.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<PaymentIntentDetails> createIntent(int orderId) async {
    final response = await http.post(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/Intent'),
      headers: createHeaders(),
      body: jsonEncode({'orderId': orderId}),
    );
    validateResponse(response);

    return PaymentIntentDetails.fromJson(jsonDecode(response.body));
  }

  Future<Payment> readByOrder(int orderId) async {
    final response = await http.get(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/ByOrder/$orderId'),
      headers: createHeaders(),
    );
    validateResponse(response);

    return Payment.fromJson(jsonDecode(response.body));
  }
}
