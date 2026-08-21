import 'package:json_annotation/json_annotation.dart';

part 'payment.g.dart';

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
  });

  factory Payment.fromJson(Map<String, dynamic> json) => _$PaymentFromJson(json);

  Map<String, dynamic> toJson() => _$PaymentToJson(this);
}

@JsonSerializable()
class PaymentIntentDetails {
  int? paymentId;
  int? orderId;
  double? amountCharged;
  String? currency;
  String? clientSecret;

  PaymentIntentDetails({
    this.paymentId,
    this.orderId,
    this.amountCharged,
    this.currency,
    this.clientSecret,
  });

  factory PaymentIntentDetails.fromJson(Map<String, dynamic> json) =>
      _$PaymentIntentDetailsFromJson(json);

  Map<String, dynamic> toJson() => _$PaymentIntentDetailsToJson(this);
}

@JsonSerializable()
class PaymentConfig {
  String? publishableKey;
  bool? isLiveGateway;

  PaymentConfig({this.publishableKey, this.isLiveGateway});

  factory PaymentConfig.fromJson(Map<String, dynamic> json) =>
      _$PaymentConfigFromJson(json);

  Map<String, dynamic> toJson() => _$PaymentConfigToJson(this);
}
