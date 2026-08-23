// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'order.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

OrderStatusHistoryEntry _$OrderStatusHistoryEntryFromJson(
  Map<String, dynamic> json,
) => OrderStatusHistoryEntry(
  fromStatus: json['fromStatus'] as String?,
  toStatus: json['toStatus'] as String?,
  changedBy: json['changedBy'] as String?,
  changedAt: json['changedAt'] == null
      ? null
      : DateTime.parse(json['changedAt'] as String),
  reason: json['reason'] as String?,
);

Map<String, dynamic> _$OrderStatusHistoryEntryToJson(
  OrderStatusHistoryEntry instance,
) => <String, dynamic>{
  'fromStatus': instance.fromStatus,
  'toStatus': instance.toStatus,
  'changedBy': instance.changedBy,
  'changedAt': instance.changedAt?.toIso8601String(),
  'reason': instance.reason,
};

Order _$OrderFromJson(Map<String, dynamic> json) => Order(
  id: (json['id'] as num?)?.toInt(),
  passengerName: json['passengerName'] as String?,
  taxiCompanyId: (json['taxiCompanyId'] as num?)?.toInt(),
  companyName: json['companyName'] as String?,
  driverName: json['driverName'] as String?,
  carPlate: json['carPlate'] as String?,
  pickupLat: (json['pickupLat'] as num?)?.toDouble(),
  pickupLng: (json['pickupLng'] as num?)?.toDouble(),
  destinationLat: (json['destinationLat'] as num?)?.toDouble(),
  destinationLng: (json['destinationLng'] as num?)?.toDouble(),
  pickupAddress: json['pickupAddress'] as String?,
  destinationAddress: json['destinationAddress'] as String?,
  distanceKm: (json['distanceKm'] as num?)?.toDouble(),
  fare: (json['fare'] as num?)?.toDouble(),
  status: json['status'] as String?,
  isGeneralOrder: json['isGeneralOrder'] as bool?,
  createdAt: json['createdAt'] == null
      ? null
      : DateTime.parse(json['createdAt'] as String),
  acceptedAt: json['acceptedAt'] == null
      ? null
      : DateTime.parse(json['acceptedAt'] as String),
  completedAt: json['completedAt'] == null
      ? null
      : DateTime.parse(json['completedAt'] as String),
  cancelledAt: json['cancelledAt'] == null
      ? null
      : DateTime.parse(json['cancelledAt'] as String),
  cancellationReason: json['cancellationReason'] as String?,
  statusHistory: (json['statusHistory'] as List<dynamic>?)
      ?.map((e) => OrderStatusHistoryEntry.fromJson(e as Map<String, dynamic>))
      .toList(),
);

Map<String, dynamic> _$OrderToJson(Order instance) => <String, dynamic>{
  'id': instance.id,
  'passengerName': instance.passengerName,
  'taxiCompanyId': instance.taxiCompanyId,
  'companyName': instance.companyName,
  'driverName': instance.driverName,
  'carPlate': instance.carPlate,
  'pickupLat': instance.pickupLat,
  'pickupLng': instance.pickupLng,
  'destinationLat': instance.destinationLat,
  'destinationLng': instance.destinationLng,
  'pickupAddress': instance.pickupAddress,
  'destinationAddress': instance.destinationAddress,
  'distanceKm': instance.distanceKm,
  'fare': instance.fare,
  'status': instance.status,
  'isGeneralOrder': instance.isGeneralOrder,
  'createdAt': instance.createdAt?.toIso8601String(),
  'acceptedAt': instance.acceptedAt?.toIso8601String(),
  'completedAt': instance.completedAt?.toIso8601String(),
  'cancelledAt': instance.cancelledAt?.toIso8601String(),
  'cancellationReason': instance.cancellationReason,
  'statusHistory': instance.statusHistory,
};
