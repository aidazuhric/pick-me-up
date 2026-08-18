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
  contactPhone: json['contactPhone'] as String?,
  startingPrice: (json['startingPrice'] as num?)?.toDouble(),
  pricePerKm: (json['pricePerKm'] as num?)?.toDouble(),
  logoImageUrl: json['logoImageUrl'] as String?,
  isActive: json['isActive'] as bool?,
);

Map<String, dynamic> _$TaxiCompanyToJson(TaxiCompany instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'hqAddress': instance.hqAddress,
      'cityName': instance.cityName,
      'contactPhone': instance.contactPhone,
      'startingPrice': instance.startingPrice,
      'pricePerKm': instance.pricePerKm,
      'logoImageUrl': instance.logoImageUrl,
      'isActive': instance.isActive,
    };
