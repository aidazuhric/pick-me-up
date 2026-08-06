// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'payment.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

PaymentRefund _$PaymentRefundFromJson(Map<String, dynamic> json) =>
    PaymentRefund(
      refundedAmount: (json['refundedAmount'] as num?)?.toDouble(),
      reason: json['reason'] as String?,
      refundedAt: json['refundedAt'] == null
          ? null
          : DateTime.parse(json['refundedAt'] as String),
    );

Map<String, dynamic> _$PaymentRefundToJson(PaymentRefund instance) =>
    <String, dynamic>{
      'refundedAmount': instance.refundedAmount,
      'reason': instance.reason,
      'refundedAt': instance.refundedAt?.toIso8601String(),
    };

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
  refunds: (json['refunds'] as List<dynamic>?)
      ?.map((e) => PaymentRefund.fromJson(e as Map<String, dynamic>))
      .toList(),
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
  'refunds': instance.refunds,
};
