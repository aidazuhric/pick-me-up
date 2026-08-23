import 'package:signalr_netcore/signalr_client.dart';

import '../core/config/api_config.dart';
import '../models/notification_item.dart';
import '../providers/auth_provider.dart';

class NotificationHubClient {
  static const String _receiveNotification = 'ReceiveNotification';

  HubConnection? _connection;
  bool _isStarting = false;

  bool get isConnected =>
      _connection?.state == HubConnectionState.Connected;

  Future<void> start(void Function(NotificationItem) onNotification) async {
    if (_isStarting || isConnected || AuthProvider.accessToken == null) {
      return;
    }

    _isStarting = true;

    try {
      final base = ApiConfig.baseUrl.endsWith('/')
          ? ApiConfig.baseUrl
          : '${ApiConfig.baseUrl}/';

      final connection = HubConnectionBuilder()
          .withUrl(
            '${base}hubs/notifications',
            options: HttpConnectionOptions(
              accessTokenFactory: () async => AuthProvider.accessToken ?? '',
            ),
          )
          .withAutomaticReconnect(retryDelays: [0, 2000, 5000, 10000, 30000])
          .build();

      connection.on(_receiveNotification, (arguments) {
        final payload = arguments?.isNotEmpty == true ? arguments!.first : null;

        if (payload is Map) {
          onNotification(
            NotificationItem.fromPush(Map<String, dynamic>.from(payload)),
          );
        }
      });

      await connection.start();

      _connection = connection;
    } catch (_) {
      _connection = null;
    } finally {
      _isStarting = false;
    }
  }

  Future<void> stop() async {
    final connection = _connection;
    _connection = null;

    if (connection == null) {
      return;
    }

    try {
      await connection.stop();
    } catch (_) {
      return;
    }
  }
}
