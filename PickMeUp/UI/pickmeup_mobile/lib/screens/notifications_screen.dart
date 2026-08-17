import 'package:flutter/material.dart';

import 'placeholder_screen.dart';

class NotificationsScreen extends StatelessWidget {
  const NotificationsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Notifications')),
      body: const PlaceholderScreen(
        icon: Icons.notifications_none,
        title: 'No notifications yet',
        description:
            'Ride and payment updates will appear here once real-time '
            'notifications are switched on.',
      ),
    );
  }
}
