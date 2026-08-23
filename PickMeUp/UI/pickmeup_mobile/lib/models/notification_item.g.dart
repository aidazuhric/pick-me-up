// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'notification_item.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

NotificationItem _$NotificationItemFromJson(Map<String, dynamic> json) =>
    NotificationItem(
      id: (json['id'] as num?)?.toInt(),
      title: json['title'] as String?,
      body: json['body'] as String?,
      category: json['category'] as String?,
      orderId: (json['orderId'] as num?)?.toInt(),
      isRead: json['isRead'] as bool?,
      readAt: json['readAt'] == null
          ? null
          : DateTime.parse(json['readAt'] as String),
      createdAt: json['createdAt'] == null
          ? null
          : DateTime.parse(json['createdAt'] as String),
    );

Map<String, dynamic> _$NotificationItemToJson(NotificationItem instance) =>
    <String, dynamic>{
      'id': instance.id,
      'title': instance.title,
      'body': instance.body,
      'category': instance.category,
      'orderId': instance.orderId,
      'isRead': instance.isRead,
      'readAt': instance.readAt?.toIso8601String(),
      'createdAt': instance.createdAt?.toIso8601String(),
    };
