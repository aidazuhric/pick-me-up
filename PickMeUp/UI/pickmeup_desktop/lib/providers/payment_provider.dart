import '../models/payment.dart';
import 'base_provider.dart';

class PaymentProvider extends BaseProvider<Payment> {
  PaymentProvider() : super('Payments');

  @override
  Payment fromJson(dynamic data) =>
      Payment.fromJson(data as Map<String, dynamic>);
}
