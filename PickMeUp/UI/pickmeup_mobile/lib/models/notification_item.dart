import 'package:json_annotation/json_annotation.dart';

part 'notification_item.g.dart';

@JsonSerializable()
class NotificationItem {
  int? id;
  String? title;
  String? body;
  String? category;
  int? orderId;
  bool? isRead;
  DateTime? readAt;
  DateTime? createdAt;

  NotificationItem({
    this.id,
    this.title,
    this.body,
    this.category,
    this.orderId,
    this.isRead,
    this.readAt,
    this.createdAt,
  });

  factory NotificationItem.fromJson(Map<String, dynamic> json) =>
      _$NotificationItemFromJson(json);

  factory NotificationItem.fromPush(Map<String, dynamic> json) {
    return NotificationItem(
      id: json['notificationId'] as int?,
      title: json['title'] as String?,
      body: json['body'] as String?,
      category: json['category'] as String?,
      orderId: json['orderId'] as int?,
      isRead: json['isRead'] as bool? ?? false,
      createdAt: json['createdAt'] == null
          ? null
          : DateTime.tryParse('${json['createdAt']}'),
    );
  }

  Map<String, dynamic> toJson() => _$NotificationItemToJson(this);
}
