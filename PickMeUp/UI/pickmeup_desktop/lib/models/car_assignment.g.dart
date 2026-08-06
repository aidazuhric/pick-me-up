// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'car_assignment.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CarAssignment _$CarAssignmentFromJson(Map<String, dynamic> json) =>
    CarAssignment(
      id: (json['id'] as num?)?.toInt(),
      carPlate: json['carPlate'] as String?,
      carMake: json['carMake'] as String?,
      carModel: json['carModel'] as String?,
      categoryName: json['categoryName'] as String?,
      driverName: json['driverName'] as String?,
      companyName: json['companyName'] as String?,
      assignedFrom: json['assignedFrom'] == null
          ? null
          : DateTime.parse(json['assignedFrom'] as String),
      assignedTo: json['assignedTo'] == null
          ? null
          : DateTime.parse(json['assignedTo'] as String),
      isOngoing: json['isOngoing'] as bool?,
      isActive: json['isActive'] as bool?,
    );

Map<String, dynamic> _$CarAssignmentToJson(CarAssignment instance) =>
    <String, dynamic>{
      'id': instance.id,
      'carPlate': instance.carPlate,
      'carMake': instance.carMake,
      'carModel': instance.carModel,
      'categoryName': instance.categoryName,
      'driverName': instance.driverName,
      'companyName': instance.companyName,
      'assignedFrom': instance.assignedFrom?.toIso8601String(),
      'assignedTo': instance.assignedTo?.toIso8601String(),
      'isOngoing': instance.isOngoing,
      'isActive': instance.isActive,
    };
