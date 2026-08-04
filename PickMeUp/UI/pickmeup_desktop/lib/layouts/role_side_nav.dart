import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../core/theme/app_colors.dart';
import '../providers/auth_provider.dart';
import 'nav_destination.dart';

class RoleSideNav extends StatefulWidget {
  const RoleSideNav({
    super.key,
    required this.roleLabel,
    required this.destinations,
  });

  final String roleLabel;
  final List<NavDestination> destinations;

  @override
  State<RoleSideNav> createState() => _RoleSideNavState();
}

class _RoleSideNavState extends State<RoleSideNav> {
  int _selectedIndex = 0;

  Future<void> _logout() async {
    await context.read<AuthProvider>().logout();
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();
    final selected = widget.destinations[_selectedIndex];

    return Scaffold(
      body: Row(
        children: [
          _SideNav(
            roleLabel: widget.roleLabel,
            displayName: auth.displayName ?? widget.roleLabel,
            destinations: widget.destinations,
            selectedIndex: _selectedIndex,
            onSelected: (index) => setState(() => _selectedIndex = index),
            onLogout: _logout,
          ),
          Expanded(
            child: Container(
              color: AppColors.surface,
              padding: const EdgeInsets.all(32),
              child: selected.builder(context),
            ),
          ),
        ],
      ),
    );
  }
}

class _SideNav extends StatelessWidget {
  const _SideNav({
    required this.roleLabel,
    required this.displayName,
    required this.destinations,
    required this.selectedIndex,
    required this.onSelected,
    required this.onLogout,
  });

  final String roleLabel;
  final String displayName;
  final List<NavDestination> destinations;
  final int selectedIndex;
  final ValueChanged<int> onSelected;
  final Future<void> Function() onLogout;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 260,
      color: AppColors.primaryIndigo,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const SizedBox(height: 32),
          const Center(
            child: CircleAvatar(
              radius: 40,
              backgroundColor: Colors.white24,
              child: Icon(Icons.person, size: 44, color: Colors.white),
            ),
          ),
          const SizedBox(height: 14),
          Center(
            child: Text(
              displayName,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 16,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
          Center(
            child: Text(
              roleLabel,
              style: const TextStyle(color: Colors.white70, fontSize: 13),
            ),
          ),
          const SizedBox(height: 28),
          Expanded(
            child: ListView.builder(
              itemCount: destinations.length,
              itemBuilder: (context, index) {
                final destination = destinations[index];
                final isSelected = index == selectedIndex;

                return ListTile(
                  leading: Icon(
                    destination.icon,
                    color: isSelected ? AppColors.accentCyan : Colors.white70,
                  ),
                  title: Text(
                    destination.label,
                    style: TextStyle(
                      color: isSelected ? AppColors.accentCyan : Colors.white,
                      fontWeight:
                          isSelected ? FontWeight.w600 : FontWeight.normal,
                    ),
                  ),
                  selected: isSelected,
                  onTap: () => onSelected(index),
                );
              },
            ),
          ),
          const Divider(color: Colors.white24, height: 1),
          ListTile(
            leading: const Icon(Icons.logout, color: Colors.white70),
            title: const Text(
              'Log out',
              style: TextStyle(color: Colors.white),
            ),
            onTap: onLogout,
          ),
          const SizedBox(height: 12),
        ],
      ),
    );
  }
}
