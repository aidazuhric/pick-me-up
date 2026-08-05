import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import 'reference_data_screen.dart';
import 'reference_table.dart';

class ReferenceDataHub extends StatefulWidget {
  const ReferenceDataHub({super.key});

  @override
  State<ReferenceDataHub> createState() => _ReferenceDataHubState();
}

class _ReferenceDataHubState extends State<ReferenceDataHub> {
  int _selectedIndex = 0;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final selected = referenceTables[_selectedIndex];

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text('Reference data', style: theme.textTheme.headlineSmall),
        const SizedBox(height: 4),
        Text(
          'The lookup lists the rest of the platform chooses from.',
          style: theme.textTheme.bodyMedium?.copyWith(color: Colors.black54),
        ),
        const SizedBox(height: 16),
        SingleChildScrollView(
          scrollDirection: Axis.horizontal,
          child: Row(
            children: [
              for (var index = 0; index < referenceTables.length; index++)
                Padding(
                  padding: const EdgeInsets.only(right: 10),
                  child: _TableChip(
                    table: referenceTables[index],
                    isSelected: index == _selectedIndex,
                    onSelected: () => setState(() => _selectedIndex = index),
                  ),
                ),
            ],
          ),
        ),
        const SizedBox(height: 20),
        Expanded(
          child: ReferenceDataScreen(
            key: ValueKey(selected.endpoint),
            table: selected,
          ),
        ),
      ],
    );
  }
}

class _TableChip extends StatelessWidget {
  const _TableChip({
    required this.table,
    required this.isSelected,
    required this.onSelected,
  });

  final ReferenceTable table;
  final bool isSelected;
  final VoidCallback onSelected;

  @override
  Widget build(BuildContext context) {
    return ChoiceChip(
      selected: isSelected,
      onSelected: (_) => onSelected(),
      avatar: Icon(
        table.icon,
        size: 18,
        color: isSelected ? Colors.white : AppColors.primaryIndigo,
      ),
      label: Text(table.title),
      labelStyle: TextStyle(
        color: isSelected ? Colors.white : Colors.black87,
        fontWeight: isSelected ? FontWeight.w600 : FontWeight.normal,
      ),
      selectedColor: AppColors.primaryIndigo,
      backgroundColor: Colors.white,
      showCheckmark: false,
      side: BorderSide(
        color: isSelected ? AppColors.primaryIndigo : AppColors.divider,
      ),
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
    );
  }
}
