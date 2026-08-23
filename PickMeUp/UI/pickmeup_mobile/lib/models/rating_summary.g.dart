// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'rating_summary.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

RatingSummary _$RatingSummaryFromJson(Map<String, dynamic> json) =>
    RatingSummary(
      id: (json['id'] as num?)?.toInt(),
      name: json['name'] as String?,
      averageRating: (json['averageRating'] as num?)?.toDouble(),
      reviewCount: (json['reviewCount'] as num?)?.toInt(),
    );

Map<String, dynamic> _$RatingSummaryToJson(RatingSummary instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'averageRating': instance.averageRating,
      'reviewCount': instance.reviewCount,
    };
