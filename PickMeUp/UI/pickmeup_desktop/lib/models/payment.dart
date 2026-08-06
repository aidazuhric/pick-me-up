import 'package:json_annotation/json_annotation.dart';

part 'payment.g.dart';

@JsonSerializable()
class PaymentRefund {
  double? refundedAmount;
  String? reason;
  DateTime? refundedAt;

  PaymentRefund({this.refundedAmount, this.reason, this.refundedAt});

  factory PaymentRefund.fromJson(Map<String, dynamic> json) =>
      _$PaymentRefundFromJson(json);

  Map<String, dynamic> toJson() => _$PaymentRefundToJson(this);
}

@JsonSerializable()
class Payment {
  int? id;
  int? orderId;
  String? passengerName;
  String? companyName;
  String? driverName;
  double? amountCharged;
  String? currency;
  String? status;
  bool? isPaid;
  double? refundedTotal;
  DateTime? createdAt;
  DateTime? paidAt;
  List<PaymentRefund>? refunds;

  Payment({
    this.id,
    this.orderId,
    this.passengerName,
    this.companyName,
    this.driverName,
    this.amountCharged,
    this.currency,
    this.status,
    this.isPaid,
    this.refundedTotal,
    this.createdAt,
    this.paidAt,
    this.refunds,
  });

  factory Payment.fromJson(Map<String, dynamic> json) =>
      _$PaymentFromJson(json);

  Map<String, dynamic> toJson() => _$PaymentToJson(this);
}
