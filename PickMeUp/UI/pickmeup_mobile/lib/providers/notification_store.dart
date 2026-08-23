import 'package:flutter/foundation.dart';

import '../models/notification_item.dart';
import '../services/notification_hub_client.dart';
import '../utils/api_client_exception.dart';
import 'notification_provider.dart';

class NotificationStore with ChangeNotifier {
  static const int pageSize = 20;

  final _provider = NotificationProvider();
  final _hub = NotificationHubClient();

  List<NotificationItem> _items = const [];
  int _unreadCount = 0;
  bool _isLoading = false;
  String? _error;

  List<NotificationItem> get items => _items;
  int get unreadCount => _unreadCount;
  bool get isLoading => _isLoading;
  String? get error => _error;
  bool get isLive => _hub.isConnected;

  Future<void> connect() async {
    await _hub.start(_onPushed);
    notifyListeners();
  }

  Future<void> disconnect() async {
    await _hub.stop();
    _items = const [];
    _unreadCount = 0;
    _error = null;
    notifyListeners();
  }

  void _onPushed(NotificationItem pushed) {
    final withoutDuplicate = _items
        .where((n) => pushed.id == null || n.id != pushed.id)
        .toList();

    _items = [pushed, ...withoutDuplicate];

    if (pushed.isRead != true) {
      _unreadCount += 1;
    }

    notifyListeners();
  }

  Future<void> refreshUnreadCount() async {
    try {
      _unreadCount = await _provider.readUnreadCount();
      notifyListeners();
    } catch (_) {
      return;
    }
  }

  Future<void> load({bool? isRead}) async {
    _isLoading = true;
    _error = null;
    notifyListeners();

    try {
      final result = await _provider.get(
        filter: {
          'Page': 1,
          'PageSize': pageSize,
          'SortBy': 'CreatedAt desc',
          'IsRead': ?isRead,
        },
      );

      _items = result.items ?? const <NotificationItem>[];
      _unreadCount = await _provider.readUnreadCount();
      _isLoading = false;
    } on ApiClientException catch (e) {
      _error = e.message;
      _isLoading = false;
    } catch (_) {
      _error = 'Could not reach the server. Check that the API is running.';
      _isLoading = false;
    }

    notifyListeners();
  }

  Future<void> markRead(NotificationItem item) async {
    final id = item.id;

    if (id == null || item.isRead == true) {
      return;
    }

    try {
      final updated = await _provider.markRead(id);

      _items = [
        for (final n in _items) if (n.id == id) updated else n,
      ];

      _unreadCount = await _provider.readUnreadCount();
      notifyListeners();
    } catch (_) {
      return;
    }
  }

  Future<bool> markAllRead() async {
    try {
      _unreadCount = await _provider.markAllRead();

      for (final n in _items) {
        n.isRead = true;
      }

      notifyListeners();
      return true;
    } catch (_) {
      return false;
    }
  }
}
