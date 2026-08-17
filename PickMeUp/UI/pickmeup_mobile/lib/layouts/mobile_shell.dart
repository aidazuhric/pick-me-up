import 'package:flutter/material.dart';

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
  Widget build(BuildContext context) {
    final destination = widget.destinations[_selectedIndex];

    return Scaffold(
      appBar: AppBar(
        title: Text(destination.label),
        actions: [
          IconButton(
            tooltip: 'Notifications',
            icon: const Icon(Icons.notifications_none),
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
