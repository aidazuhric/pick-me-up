import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';

class StarRatingInput extends StatelessWidget {
  const StarRatingInput({
    super.key,
    required this.value,
    required this.onChanged,
    this.size = 44,
  });

  final int value;
  final ValueChanged<int> onChanged;
  final double size;

  static const List<String> _labels = [
    'Tap a star to rate',
    'Poor',
    'Not great',
    'Fine',
    'Good',
    'Excellent',
  ];

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Column(
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            for (var star = 1; star <= 5; star++)
              Semantics(
                button: true,
                label: '$star ${star == 1 ? 'star' : 'stars'}',
                child: InkResponse(
                  onTap: () => onChanged(star),
                  radius: size * 0.6,
                  child: Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 4),
                    child: Icon(
                      star <= value ? Icons.star : Icons.star_border,
                      size: size,
                      color: star <= value
                          ? AppColors.accentCyan
                          : AppColors.divider,
                    ),
                  ),
                ),
              ),
          ],
        ),
        const SizedBox(height: 6),
        Text(
          _labels[value.clamp(0, 5)],
          style: theme.textTheme.bodySmall,
        ),
      ],
    );
  }
}
