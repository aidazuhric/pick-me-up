// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'taxi_company_details.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

TaxiCompanyDetails _$TaxiCompanyDetailsFromJson(Map<String, dynamic> json) =>
    TaxiCompanyDetails(
      id: (json['id'] as num?)?.toInt(),
      name: json['name'] as String?,
      cityName: json['cityName'] as String?,
      hqAddress: json['hqAddress'] as String?,
      contactPhone: json['contactPhone'] as String?,
      startingPrice: (json['startingPrice'] as num?)?.toDouble(),
      pricePerKm: (json['pricePerKm'] as num?)?.toDouble(),
      logoImageUrl: json['logoImageUrl'] as String?,
      vehicleCount: (json['vehicleCount'] as num?)?.toInt(),
      averageRating: (json['averageRating'] as num?)?.toDouble(),
      reviewCount: (json['reviewCount'] as num?)?.toInt(),
    );

Map<String, dynamic> _$TaxiCompanyDetailsToJson(TaxiCompanyDetails instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'cityName': instance.cityName,
      'hqAddress': instance.hqAddress,
      'contactPhone': instance.contactPhone,
      'startingPrice': instance.startingPrice,
      'pricePerKm': instance.pricePerKm,
      'logoImageUrl': instance.logoImageUrl,
      'vehicleCount': instance.vehicleCount,
      'averageRating': instance.averageRating,
      'reviewCount': instance.reviewCount,
    };
