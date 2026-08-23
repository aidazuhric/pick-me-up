import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/notification_item.dart';
import 'base_provider.dart';

class NotificationProvider extends BaseProvider<NotificationItem> {
  NotificationProvider() : super('Notifications');

  @override
  NotificationItem fromJson(dynamic data) =>
      NotificationItem.fromJson(data as Map<String, dynamic>);

  Future<int> readUnreadCount() async {
    final response = await http.get(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/UnreadCount'),
      headers: createHeaders(),
    );
    validateResponse(response);

    return jsonDecode(response.body)['unreadCount'] as int? ?? 0;
  }

  Future<NotificationItem> markRead(int id) async {
    final response = await http.post(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/$id/Read'),
      headers: createHeaders(),
    );
    validateResponse(response);

    return NotificationItem.fromJson(jsonDecode(response.body));
  }

  Future<int> markAllRead() async {
    final response = await http.post(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/ReadAll'),
      headers: createHeaders(),
    );
    validateResponse(response);

    return jsonDecode(response.body)['unreadCount'] as int? ?? 0;
  }
}
