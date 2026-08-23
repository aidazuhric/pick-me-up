// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'recommendation.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Recommendation _$RecommendationFromJson(Map<String, dynamic> json) =>
    Recommendation(
      taxiCompanyId: (json['taxiCompanyId'] as num?)?.toInt(),
      companyName: json['companyName'] as String?,
      cityName: json['cityName'] as String?,
      score: (json['score'] as num?)?.toDouble(),
      isInHomeCity: json['isInHomeCity'] as bool?,
      explanation: json['explanation'] as String?,
      yourOrders: (json['yourOrders'] as num?)?.toInt(),
      yourCompletedRides: (json['yourCompletedRides'] as num?)?.toInt(),
      yourAverageRating: (json['yourAverageRating'] as num?)?.toDouble(),
      completedRidesOverall: (json['completedRidesOverall'] as num?)?.toInt(),
      averageRatingOverall: (json['averageRatingOverall'] as num?)?.toDouble(),
      reviewCountOverall: (json['reviewCountOverall'] as num?)?.toInt(),
    );

Map<String, dynamic> _$RecommendationToJson(Recommendation instance) =>
    <String, dynamic>{
      'taxiCompanyId': instance.taxiCompanyId,
      'companyName': instance.companyName,
      'cityName': instance.cityName,
      'score': instance.score,
      'isInHomeCity': instance.isInHomeCity,
      'explanation': instance.explanation,
      'yourOrders': instance.yourOrders,
      'yourCompletedRides': instance.yourCompletedRides,
      'yourAverageRating': instance.yourAverageRating,
      'completedRidesOverall': instance.completedRidesOverall,
      'averageRatingOverall': instance.averageRatingOverall,
      'reviewCountOverall': instance.reviewCountOverall,
    };
