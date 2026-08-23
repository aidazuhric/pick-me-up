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

  factory Review.fromJson(Map<String, dynamic> json) => _$ReviewFromJson(json);

  Map<String, dynamic> toJson() => _$ReviewToJson(this);
}
