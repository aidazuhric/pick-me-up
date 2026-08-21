// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'payable_order.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

PayableOrder _$PayableOrderFromJson(Map<String, dynamic> json) => PayableOrder(
  orderId: (json['orderId'] as num?)?.toInt(),
  companyName: json['companyName'] as String?,
  driverName: json['driverName'] as String?,
  pickupAddress: json['pickupAddress'] as String?,
  destinationAddress: json['destinationAddress'] as String?,
  distanceKm: (json['distanceKm'] as num?)?.toDouble(),
  fare: (json['fare'] as num?)?.toDouble(),
  completedAt: json['completedAt'] == null
      ? null
      : DateTime.parse(json['completedAt'] as String),
);

Map<String, dynamic> _$PayableOrderToJson(PayableOrder instance) =>
    <String, dynamic>{
      'orderId': instance.orderId,
      'companyName': instance.companyName,
      'driverName': instance.driverName,
      'pickupAddress': instance.pickupAddress,
      'destinationAddress': instance.destinationAddress,
      'distanceKm': instance.distanceKm,
      'fare': instance.fare,
      'completedAt': instance.completedAt?.toIso8601String(),
    };
