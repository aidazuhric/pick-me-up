// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'driver_location.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

DriverLocation _$DriverLocationFromJson(Map<String, dynamic> json) =>
    DriverLocation(
      driverId: (json['driverId'] as num?)?.toInt(),
      driverName: json['driverName'] as String?,
      companyName: json['companyName'] as String?,
      carPlate: json['carPlate'] as String?,
      latitude: (json['latitude'] as num?)?.toDouble(),
      longitude: (json['longitude'] as num?)?.toDouble(),
      updatedAt: json['updatedAt'] == null
          ? null
          : DateTime.parse(json['updatedAt'] as String),
      isOnShift: json['isOnShift'] as bool?,
      isOnBreak: json['isOnBreak'] as bool?,
      isDispatchEligible: json['isDispatchEligible'] as bool?,
    );

Map<String, dynamic> _$DriverLocationToJson(DriverLocation instance) =>
    <String, dynamic>{
      'driverId': instance.driverId,
      'driverName': instance.driverName,
      'companyName': instance.companyName,
      'carPlate': instance.carPlate,
      'latitude': instance.latitude,
      'longitude': instance.longitude,
      'updatedAt': instance.updatedAt?.toIso8601String(),
      'isOnShift': instance.isOnShift,
      'isOnBreak': instance.isOnBreak,
      'isDispatchEligible': instance.isDispatchEligible,
    };
