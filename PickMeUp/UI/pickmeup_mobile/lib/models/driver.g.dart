// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'driver.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Driver _$DriverFromJson(Map<String, dynamic> json) => Driver(
  id: (json['id'] as num?)?.toInt(),
  fullName: json['fullName'] as String?,
  email: json['email'] as String?,
  phoneNumber: json['phoneNumber'] as String?,
  companyName: json['companyName'] as String?,
  hireDate: json['hireDate'] == null
      ? null
      : DateTime.parse(json['hireDate'] as String),
  resignedAt: json['resignedAt'] == null
      ? null
      : DateTime.parse(json['resignedAt'] as String),
  isActive: json['isActive'] as bool?,
);

Map<String, dynamic> _$DriverToJson(Driver instance) => <String, dynamic>{
  'id': instance.id,
  'fullName': instance.fullName,
  'email': instance.email,
  'phoneNumber': instance.phoneNumber,
  'companyName': instance.companyName,
  'hireDate': instance.hireDate?.toIso8601String(),
  'resignedAt': instance.resignedAt?.toIso8601String(),
  'isActive': instance.isActive,
};
