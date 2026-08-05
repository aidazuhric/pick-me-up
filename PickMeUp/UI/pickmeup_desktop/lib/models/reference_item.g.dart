// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'reference_item.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

ReferenceItem _$ReferenceItemFromJson(Map<String, dynamic> json) =>
    ReferenceItem(
      id: (json['id'] as num?)?.toInt(),
      name: json['name'] as String?,
      description: json['description'] as String?,
      postalCode: json['postalCode'] as String?,
      sortOrder: (json['sortOrder'] as num?)?.toInt(),
      isActive: json['isActive'] as bool?,
    );

Map<String, dynamic> _$ReferenceItemToJson(ReferenceItem instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'description': instance.description,
      'postalCode': instance.postalCode,
      'sortOrder': instance.sortOrder,
      'isActive': instance.isActive,
    };
