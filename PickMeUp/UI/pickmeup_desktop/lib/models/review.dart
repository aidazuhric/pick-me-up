import 'package:json_annotation/json_annotation.dart';

part 'review.g.dart';

@JsonSerializable()
class Review {
  int? id;
  int? orderId;
  String? authorName;
  String? companyName;
  String? driverName;
  int? rating;
  String? comment;
  DateTime? createdAt;

  Review({
    this.id,
    this.orderId,
    this.authorName,
    this.companyName,
    this.driverName,
    this.rating,
    this.comment,
    this.createdAt,
  });

  String get target => driverName == null
      ? (companyName ?? '')
      : (companyName == null ? driverName! : '$companyName · $driverName');

  factory Review.fromJson(Map<String, dynamic> json) => _$ReviewFromJson(json);

  Map<String, dynamic> toJson() => _$ReviewToJson(this);
}

@JsonSerializable()
class RatingSummary {
  int? id;
  String? name;
  String? companyName;
  double? averageRating;
  int? reviewCount;

  RatingSummary({
    this.id,
    this.name,
    this.companyName,
    this.averageRating,
    this.reviewCount,
  });

  factory RatingSummary.fromJson(Map<String, dynamic> json) =>
      _$RatingSummaryFromJson(json);

  Map<String, dynamic> toJson() => _$RatingSummaryToJson(this);
}
