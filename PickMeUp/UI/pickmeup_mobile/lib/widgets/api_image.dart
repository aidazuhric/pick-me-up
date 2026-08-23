import 'package:flutter/material.dart';

import '../core/config/api_config.dart';
import '../core/theme/app_colors.dart';
import '../providers/auth_provider.dart';

class ApiImage extends StatelessWidget {
  const ApiImage.logo({
    super.key,
    required this.url,
    this.size = 48,
    this.fallbackIcon = Icons.local_taxi,
  });

  final String? url;
  final double size;
  final IconData fallbackIcon;

  static String? resolve(String? path) {
    if (path == null || path.trim().isEmpty) {
      return null;
    }

    final trimmed = path.trim();

    if (trimmed.startsWith('http://') || trimmed.startsWith('https://')) {
      return trimmed;
    }

    final base = ApiConfig.baseUrl.endsWith('/')
        ? ApiConfig.baseUrl
        : '${ApiConfig.baseUrl}/';

    return '$base${trimmed.startsWith('/') ? trimmed.substring(1) : trimmed}';
  }

  static Map<String, String> authHeaders() {
    final token = AuthProvider.accessToken;
    return token == null ? const {} : {'Authorization': 'Bearer $token'};
  }

  @override
  Widget build(BuildContext context) {
    final resolved = resolve(url);

    if (resolved == null) {
      return _placeholder();
    }

    return Container(
      width: size,
      height: size,
      decoration: const BoxDecoration(
        color: AppColors.sectionTint,
        shape: BoxShape.circle,
      ),
      clipBehavior: Clip.antiAlias,
      child: Image.network(
        resolved,
        headers: authHeaders(),
        width: size,
        height: size,
        fit: BoxFit.contain,
        errorBuilder: (_, _, _) => _placeholder(),
        loadingBuilder: (context, child, progress) =>
            progress == null ? child : _placeholder(),
      ),
    );
  }

  Widget _placeholder() {
    return Container(
      width: size,
      height: size,
      decoration: const BoxDecoration(
        color: AppColors.sectionTint,
        shape: BoxShape.circle,
      ),
      child: Icon(
        fallbackIcon,
        size: size * 0.5,
        color: AppColors.mobileHeader,
      ),
    );
  }
}
