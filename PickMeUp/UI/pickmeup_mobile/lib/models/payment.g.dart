// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'payment.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Payment _$PaymentFromJson(Map<String, dynamic> json) => Payment(
  id: (json['id'] as num?)?.toInt(),
  orderId: (json['orderId'] as num?)?.toInt(),
  passengerName: json['passengerName'] as String?,
  companyName: json['companyName'] as String?,
  driverName: json['driverName'] as String?,
  amountCharged: (json['amountCharged'] as num?)?.toDouble(),
  currency: json['currency'] as String?,
  status: json['status'] as String?,
  isPaid: json['isPaid'] as bool?,
  refundedTotal: (json['refundedTotal'] as num?)?.toDouble(),
  createdAt: json['createdAt'] == null
      ? null
      : DateTime.parse(json['createdAt'] as String),
  paidAt: json['paidAt'] == null
      ? null
      : DateTime.parse(json['paidAt'] as String),
);

Map<String, dynamic> _$PaymentToJson(Payment instance) => <String, dynamic>{
  'id': instance.id,
  'orderId': instance.orderId,
  'passengerName': instance.passengerName,
  'companyName': instance.companyName,
  'driverName': instance.driverName,
  'amountCharged': instance.amountCharged,
  'currency': instance.currency,
  'status': instance.status,
  'isPaid': instance.isPaid,
  'refundedTotal': instance.refundedTotal,
  'createdAt': instance.createdAt?.toIso8601String(),
  'paidAt': instance.paidAt?.toIso8601String(),
};

PaymentIntentDetails _$PaymentIntentDetailsFromJson(
  Map<String, dynamic> json,
) => PaymentIntentDetails(
  paymentId: (json['paymentId'] as num?)?.toInt(),
  orderId: (json['orderId'] as num?)?.toInt(),
  amountCharged: (json['amountCharged'] as num?)?.toDouble(),
  currency: json['currency'] as String?,
  clientSecret: json['clientSecret'] as String?,
);

Map<String, dynamic> _$PaymentIntentDetailsToJson(
  PaymentIntentDetails instance,
) => <String, dynamic>{
  'paymentId': instance.paymentId,
  'orderId': instance.orderId,
  'amountCharged': instance.amountCharged,
  'currency': instance.currency,
  'clientSecret': instance.clientSecret,
};

PaymentConfig _$PaymentConfigFromJson(Map<String, dynamic> json) =>
    PaymentConfig(
      publishableKey: json['publishableKey'] as String?,
      isLiveGateway: json['isLiveGateway'] as bool?,
    );

Map<String, dynamic> _$PaymentConfigToJson(PaymentConfig instance) =>
    <String, dynamic>{
      'publishableKey': instance.publishableKey,
      'isLiveGateway': instance.isLiveGateway,
    };
