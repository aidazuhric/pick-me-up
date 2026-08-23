import 'package:json_annotation/json_annotation.dart';

part 'rating_summary.g.dart';

@JsonSerializable()
class RatingSummary {
  int? id;
  String? name;
  double? averageRating;
  int? reviewCount;

  RatingSummary({this.id, this.name, this.averageRating, this.reviewCount});

  factory RatingSummary.fromJson(Map<String, dynamic> json) =>
      _$RatingSummaryFromJson(json);

  Map<String, dynamic> toJson() => _$RatingSummaryToJson(this);
}
