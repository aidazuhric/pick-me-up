import 'package:json_annotation/json_annotation.dart';

part 'recommendation.g.dart';

@JsonSerializable()
class Recommendation {
  int? taxiCompanyId;
  String? companyName;
  String? cityName;
  double? score;
  bool? isInHomeCity;
  String? explanation;
  int? yourOrders;
  int? yourCompletedRides;
  double? yourAverageRating;
  int? completedRidesOverall;
  double? averageRatingOverall;
  int? reviewCountOverall;

  Recommendation({
    this.taxiCompanyId,
    this.companyName,
    this.cityName,
    this.score,
    this.isInHomeCity,
    this.explanation,
    this.yourOrders,
    this.yourCompletedRides,
    this.yourAverageRating,
    this.completedRidesOverall,
    this.averageRatingOverall,
    this.reviewCountOverall,
  });

  bool get isPersonal => (yourCompletedRides ?? 0) > 0 || (yourOrders ?? 0) > 0;

  factory Recommendation.fromJson(Map<String, dynamic> json) =>
      _$RecommendationFromJson(json);

  Map<String, dynamic> toJson() => _$RecommendationToJson(this);
}
