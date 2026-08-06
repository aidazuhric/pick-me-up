import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../core/theme/app_colors.dart';
import '../core/theme/app_spacing.dart';
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
              padding: const EdgeInsets.all(AppSpacing.pagePadding),
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
      width: 248,
      color: AppColors.primaryIndigo,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const SizedBox(height: AppSpacing.xl),
          const Center(
            child: CircleAvatar(
              radius: 34,
              backgroundColor: Colors.white24,
              child: Icon(Icons.person, size: 36, color: Colors.white),
            ),
          ),
          const SizedBox(height: AppSpacing.md),
          Center(
            child: Text(
              displayName,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 15,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
          const SizedBox(height: 2),
          Center(
            child: Text(
              roleLabel.toUpperCase(),
              style: const TextStyle(
                color: Colors.white70,
                fontSize: 11,
                letterSpacing: 1.1,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
          const SizedBox(height: AppSpacing.lg),
          Expanded(
            child: ListView.builder(
              padding: const EdgeInsets.symmetric(horizontal: AppSpacing.sm),
              itemCount: destinations.length,
              itemBuilder: (context, index) {
                final destination = destinations[index];
                final isSelected = index == selectedIndex;

                return Padding(
                  padding: const EdgeInsets.only(bottom: 2),
                  child: ListTile(
                    dense: true,
                    visualDensity: VisualDensity.compact,
                    horizontalTitleGap: AppSpacing.sm,
                    contentPadding:
                        const EdgeInsets.symmetric(horizontal: AppSpacing.md),
                    shape: RoundedRectangleBorder(
                      borderRadius:
                          BorderRadius.circular(AppSpacing.controlRadius),
                    ),
                    hoverColor: Colors.white10,
                    selectedTileColor: AppColors.primaryIndigoDark,
                    leading: Icon(
                      destination.icon,
                      size: 20,
                      color: isSelected ? AppColors.accentCyan : Colors.white70,
                    ),
                    title: Text(
                      destination.label,
                      style: TextStyle(
                        fontSize: 13,
                        color: isSelected ? AppColors.accentCyan : Colors.white,
                        fontWeight:
                            isSelected ? FontWeight.w600 : FontWeight.normal,
                      ),
                    ),
                    selected: isSelected,
                    onTap: () => onSelected(index),
                  ),
                );
              },
            ),
          ),
          const Divider(color: Colors.white24, height: 1),
          Padding(
            padding: const EdgeInsets.symmetric(
              horizontal: AppSpacing.sm,
              vertical: AppSpacing.sm,
            ),
            child: ListTile(
              dense: true,
              visualDensity: VisualDensity.compact,
              horizontalTitleGap: AppSpacing.sm,
              contentPadding:
                  const EdgeInsets.symmetric(horizontal: AppSpacing.md),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
              ),
              hoverColor: Colors.white10,
              leading: const Icon(Icons.logout, size: 20, color: Colors.white70),
              title: const Text(
                'Log out',
                style: TextStyle(fontSize: 13, color: Colors.white),
              ),
              onTap: onLogout,
            ),
          ),
        ],
      ),
    );
  }
}
