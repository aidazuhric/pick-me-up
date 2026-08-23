import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../core/theme/app_colors.dart';
import '../providers/notification_store.dart';
import '../screens/notifications_screen.dart';
import '../widgets/mobile_drawer.dart';
import 'nav_destination.dart';

class MobileShell extends StatefulWidget {
  const MobileShell({
    super.key,
    required this.destinations,
    required this.roleLabel,
  });

  final List<MobileDestination> destinations;
  final String roleLabel;

  @override
  State<MobileShell> createState() => _MobileShellState();
}

class _MobileShellState extends State<MobileShell> {
  int _selectedIndex = 0;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      final store = context.read<NotificationStore>();
      store.refreshUnreadCount();
      store.connect();
    });
  }

  @override
  Widget build(BuildContext context) {
    final destination = widget.destinations[_selectedIndex];

    return Scaffold(
      appBar: AppBar(
        title: Text(destination.label),
        actions: [
          _NotificationBell(
            onPressed: () => Navigator.of(context).push(
              MaterialPageRoute<void>(
                builder: (_) => const NotificationsScreen(),
              ),
            ),
          ),
        ],
      ),
      drawer: MobileDrawer(
        destinations: widget.destinations,
        selectedIndex: _selectedIndex,
        roleLabel: widget.roleLabel,
        onSelectedIndexChanged: (index) =>
            setState(() => _selectedIndex = index),
      ),
      body: destination.builder(context),
    );
  }
}

class _NotificationBell extends StatelessWidget {
  const _NotificationBell({required this.onPressed});

  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    final unread = context.watch<NotificationStore>().unreadCount;

    return Stack(
      alignment: Alignment.center,
      children: [
        IconButton(
          tooltip: 'Notifications',
          icon: const Icon(Icons.notifications_none),
          onPressed: onPressed,
        ),
        if (unread > 0)
          Positioned(
            top: 8,
            right: 6,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 5, vertical: 1),
              constraints: const BoxConstraints(minWidth: 18),
              decoration: BoxDecoration(
                color: AppColors.danger,
                borderRadius: BorderRadius.circular(9),
                border: Border.all(color: AppColors.mobileHeader, width: 1.5),
              ),
              child: Text(
                unread > 99 ? '99+' : '$unread',
                textAlign: TextAlign.center,
                style: const TextStyle(
                  color: Colors.white,
                  fontSize: 10,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ),
          ),
      ],
    );
  }
}
