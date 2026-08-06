// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'car.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Car _$CarFromJson(Map<String, dynamic> json) => Car(
  id: (json['id'] as num?)?.toInt(),
  make: json['make'] as String?,
  model: json['model'] as String?,
  plate: json['plate'] as String?,
  year: (json['year'] as num?)?.toInt(),
  seats: (json['seats'] as num?)?.toInt(),
  categoryName: json['categoryName'] as String?,
  companyName: json['companyName'] as String?,
  photoImageUrl: json['photoImageUrl'] as String?,
  isActive: json['isActive'] as bool?,
);

Map<String, dynamic> _$CarToJson(Car instance) => <String, dynamic>{
  'id': instance.id,
  'make': instance.make,
  'model': instance.model,
  'plate': instance.plate,
  'year': instance.year,
  'seats': instance.seats,
  'categoryName': instance.categoryName,
  'companyName': instance.companyName,
  'photoImageUrl': instance.photoImageUrl,
  'isActive': instance.isActive,
};
