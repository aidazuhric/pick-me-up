import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';
import '../core/theme/app_spacing.dart';

class AuthHeader extends StatelessWidget {
  const AuthHeader({super.key, required this.title, required this.subtitle});

  final String title;
  final String subtitle;

  @override
  Widget build(BuildContext context) {
    final viewport = MediaQuery.of(context).size.height;
    final isShort = viewport < 520;

    final height = isShort
        ? (viewport * 0.38).clamp(150.0, AppSpacing.headerHeight)
        : AppSpacing.headerHeight;

    return ClipPath(
      clipper: _CloudClipper(),
      child: Container(
        width: double.infinity,
        height: height,
        color: AppColors.mobileHeader,
        padding: EdgeInsets.only(bottom: isShort ? 24 : 42),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              Icons.local_taxi,
              size: isShort ? 34 : 56,
              color: Colors.white,
            ),
            SizedBox(height: isShort ? AppSpacing.xs : AppSpacing.md),
            Text(
              title,
              style: TextStyle(
                color: Colors.white,
                fontSize: isShort ? 22 : 28,
                fontWeight: FontWeight.w700,
                letterSpacing: 0.4,
              ),
            ),
            if (!isShort) ...[
              const SizedBox(height: AppSpacing.xs),
              Text(
                subtitle,
                style: const TextStyle(color: Colors.white70, fontSize: 14),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

class _CloudClipper extends CustomClipper<Path> {
  @override
  Path getClip(Size size) {
    final path = Path()..lineTo(0, size.height - 48);

    path.quadraticBezierTo(
      size.width * 0.25,
      size.height,
      size.width * 0.5,
      size.height - 22,
    );
    path.quadraticBezierTo(
      size.width * 0.78,
      size.height - 48,
      size.width,
      size.height - 16,
    );
    path.lineTo(size.width, 0);

    return path..close();
  }

  @override
  bool shouldReclip(covariant CustomClipper<Path> oldClipper) => false;
}
