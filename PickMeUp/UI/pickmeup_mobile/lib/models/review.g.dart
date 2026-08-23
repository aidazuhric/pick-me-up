// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'review.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Review _$ReviewFromJson(Map<String, dynamic> json) => Review(
  id: (json['id'] as num?)?.toInt(),
  orderId: (json['orderId'] as num?)?.toInt(),
  authorName: json['authorName'] as String?,
  companyName: json['companyName'] as String?,
  driverName: json['driverName'] as String?,
  rating: (json['rating'] as num?)?.toInt(),
  comment: json['comment'] as String?,
  createdAt: json['createdAt'] == null
      ? null
      : DateTime.parse(json['createdAt'] as String),
);

Map<String, dynamic> _$ReviewToJson(Review instance) => <String, dynamic>{
  'id': instance.id,
  'orderId': instance.orderId,
  'authorName': instance.authorName,
  'companyName': instance.companyName,
  'driverName': instance.driverName,
  'rating': instance.rating,
  'comment': instance.comment,
  'createdAt': instance.createdAt?.toIso8601String(),
};
