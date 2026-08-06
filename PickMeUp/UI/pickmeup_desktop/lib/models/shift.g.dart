// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'shift.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Shift _$ShiftFromJson(Map<String, dynamic> json) => Shift(
  id: (json['id'] as num?)?.toInt(),
  driverName: json['driverName'] as String?,
  companyName: json['companyName'] as String?,
  startedAt: json['startedAt'] == null
      ? null
      : DateTime.parse(json['startedAt'] as String),
  endedAt: json['endedAt'] == null
      ? null
      : DateTime.parse(json['endedAt'] as String),
  isOngoing: json['isOngoing'] as bool?,
  isOnBreak: json['isOnBreak'] as bool?,
  durationMinutes: (json['durationMinutes'] as num?)?.toInt(),
  breakMinutes: (json['breakMinutes'] as num?)?.toInt(),
  workedMinutes: (json['workedMinutes'] as num?)?.toInt(),
  ordersHandled: (json['ordersHandled'] as num?)?.toInt(),
);

Map<String, dynamic> _$ShiftToJson(Shift instance) => <String, dynamic>{
  'id': instance.id,
  'driverName': instance.driverName,
  'companyName': instance.companyName,
  'startedAt': instance.startedAt?.toIso8601String(),
  'endedAt': instance.endedAt?.toIso8601String(),
  'isOngoing': instance.isOngoing,
  'isOnBreak': instance.isOnBreak,
  'durationMinutes': instance.durationMinutes,
  'breakMinutes': instance.breakMinutes,
  'workedMinutes': instance.workedMinutes,
  'ordersHandled': instance.ordersHandled,
};
