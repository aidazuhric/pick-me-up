// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'taxi_company.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

TaxiCompany _$TaxiCompanyFromJson(Map<String, dynamic> json) => TaxiCompany(
  id: (json['id'] as num?)?.toInt(),
  name: json['name'] as String?,
  hqAddress: json['hqAddress'] as String?,
  cityName: json['cityName'] as String?,
  contactEmail: json['contactEmail'] as String?,
  contactPhone: json['contactPhone'] as String?,
  ownerName: json['ownerName'] as String?,
  startingPrice: (json['startingPrice'] as num?)?.toDouble(),
  pricePerKm: (json['pricePerKm'] as num?)?.toDouble(),
  logoImageUrl: json['logoImageUrl'] as String?,
  isActive: json['isActive'] as bool?,
  createdAt: json['createdAt'] == null
      ? null
      : DateTime.parse(json['createdAt'] as String),
  updatedAt: json['updatedAt'] == null
      ? null
      : DateTime.parse(json['updatedAt'] as String),
  initialLoginUsername: json['initialLoginUsername'] as String?,
);

Map<String, dynamic> _$TaxiCompanyToJson(TaxiCompany instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'hqAddress': instance.hqAddress,
      'cityName': instance.cityName,
      'contactEmail': instance.contactEmail,
      'contactPhone': instance.contactPhone,
      'ownerName': instance.ownerName,
      'startingPrice': instance.startingPrice,
      'pricePerKm': instance.pricePerKm,
      'logoImageUrl': instance.logoImageUrl,
      'isActive': instance.isActive,
      'createdAt': instance.createdAt?.toIso8601String(),
      'updatedAt': instance.updatedAt?.toIso8601String(),
      'initialLoginUsername': instance.initialLoginUsername,
    };
