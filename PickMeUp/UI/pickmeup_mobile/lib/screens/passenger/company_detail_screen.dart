import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/review.dart';
import '../../models/taxi_company.dart';
import '../../models/taxi_company_details.dart';
import '../../providers/review_provider.dart';
import '../../providers/taxi_company_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/formats.dart';
import '../../widgets/api_image.dart';
import '../../widgets/rating_stars.dart';
import 'order_from_company_screen.dart';

class CompanyDetailScreen extends StatefulWidget {
  const CompanyDetailScreen({super.key, required this.companyId});

  final int companyId;

  static Future<void> open(BuildContext context, int companyId) {
    return Navigator.of(context).push(
      MaterialPageRoute<void>(
        builder: (_) => CompanyDetailScreen(companyId: companyId),
      ),
    );
  }

  @override
  State<CompanyDetailScreen> createState() => _CompanyDetailScreenState();
}

class _CompanyDetailScreenState extends State<CompanyDetailScreen> {
  final _companies = TaxiCompanyProvider();
  final _reviews = ReviewProvider();

  TaxiCompanyDetails? _details;
  List<Review> _companyReviews = const [];
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final details = await _companies.readDetails(widget.companyId);
      final reviews = await _reviews.get(
        filter: {
          'TargetCompanyId': widget.companyId,
          'Page': 1,
          'PageSize': 5,
          'SortBy': 'CreatedAt desc',
        },
      );

      if (!mounted) return;

      setState(() {
        _details = details;
        _companyReviews = reviews.items ?? const <Review>[];
        _isLoading = false;
      });
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.message;
        _isLoading = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _error = 'Could not reach the server. Check that the API is running.';
        _isLoading = false;
      });
    }
  }

  void _orderFromHere() {
    final details = _details;

    if (details == null) {
      return;
    }

    OrderFromCompanyScreen.open(
      context,
      TaxiCompany(
        id: details.id,
        name: details.name,
        cityName: details.cityName,
        startingPrice: details.startingPrice,
        pricePerKm: details.pricePerKm,
        logoImageUrl: details.logoImageUrl,
        isActive: true,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final details = _details;

    return Scaffold(
      appBar: AppBar(title: Text(details?.name ?? 'Company')),
      body: _buildBody(details),
      bottomNavigationBar: details == null
          ? null
          : SafeArea(
              minimum: const EdgeInsets.all(AppSpacing.screenPadding),
              child: FilledButton.icon(
                onPressed: _orderFromHere,
                icon: const Icon(Icons.local_taxi, size: 20),
                label: const Text('ORDER FROM THIS COMPANY'),
              ),
            ),
    );
  }

  Widget _buildBody(TaxiCompanyDetails? details) {
    if (_isLoading) {
      return const Center(
        child: SizedBox(
          height: 28,
          width: 28,
          child: CircularProgressIndicator(strokeWidth: 2.5),
        ),
      );
    }

    if (_error != null || details == null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Text(
            _error ?? 'This company could not be loaded.',
            textAlign: TextAlign.center,
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
        ),
      );
    }

    final theme = Theme.of(context);

    return ListView(
      padding: const EdgeInsets.all(AppSpacing.screenPadding),
      children: [
        Row(
          children: [
            ApiImage.logo(url: details.logoImageUrl, size: 68),
            const SizedBox(width: AppSpacing.md),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    details.name ?? '',
                    style: theme.textTheme.headlineSmall,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: AppSpacing.xs),
                  Text(details.cityName ?? '', style: theme.textTheme.bodySmall),
                  const SizedBox(height: AppSpacing.xs),
                  RatingStars(
                    average: details.averageRating,
                    reviewCount: details.reviewCount,
                  ),
                ],
              ),
            ),
          ],
        ),
        const SizedBox(height: AppSpacing.lg),
        Row(
          children: [
            Expanded(
              child: _Stat(
                icon: Icons.directions_car_outlined,
                label: 'Vehicles',
                value: '${details.vehicleCount ?? 0}',
              ),
            ),
            const SizedBox(width: AppSpacing.sm),
            Expanded(
              child: _Stat(
                icon: Icons.play_arrow_outlined,
                label: 'Starting price',
                value: Formats.money(details.startingPrice),
              ),
            ),
            const SizedBox(width: AppSpacing.sm),
            Expanded(
              child: _Stat(
                icon: Icons.straighten,
                label: 'Per kilometre',
                value: Formats.money(details.pricePerKm),
              ),
            ),
          ],
        ),
        const SizedBox(height: AppSpacing.lg),
        Text('Contact', style: theme.textTheme.titleMedium),
        const SizedBox(height: AppSpacing.sm),
        _InfoRow(icon: Icons.place_outlined, text: details.hqAddress ?? ''),
        const SizedBox(height: AppSpacing.xs),
        _InfoRow(icon: Icons.phone_outlined, text: details.contactPhone ?? ''),
        const SizedBox(height: AppSpacing.lg),
        Text('What passengers say', style: theme.textTheme.titleMedium),
        const SizedBox(height: AppSpacing.sm),
        if (_companyReviews.isEmpty)
          Text(
            'No reviews for this company yet.',
            style: theme.textTheme.bodySmall,
          )
        else
          for (final review in _companyReviews)
            Padding(
              padding: const EdgeInsets.only(bottom: AppSpacing.sm),
              child: _ReviewTile(review: review),
            ),
      ],
    );
  }
}

class _Stat extends StatelessWidget {
  const _Stat({required this.icon, required this.label, required this.value});

  final IconData icon;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.sm,
        vertical: AppSpacing.md,
      ),
      decoration: BoxDecoration(
        color: AppColors.sectionTint,
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      ),
      child: Column(
        children: [
          Icon(icon, size: 18, color: AppColors.mobileHeader),
          const SizedBox(height: AppSpacing.xs),
          FittedBox(
            child: Text(
              value,
              style: theme.textTheme.titleMedium,
              maxLines: 1,
            ),
          ),
          const SizedBox(height: 2),
          Text(
            label,
            textAlign: TextAlign.center,
            style: theme.textTheme.bodySmall,
            maxLines: 2,
          ),
        ],
      ),
    );
  }
}

class _InfoRow extends StatelessWidget {
  const _InfoRow({required this.icon, required this.text});

  final IconData icon;
  final String text;

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 16, color: AppColors.textMuted),
        const SizedBox(width: AppSpacing.sm),
        Expanded(
          child: Text(text, style: Theme.of(context).textTheme.bodySmall),
        ),
      ],
    );
  }
}

class _ReviewTile extends StatelessWidget {
  const _ReviewTile({required this.review});

  final Review review;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.surface,
        border: Border.all(color: AppColors.border),
        borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Expanded(
                child: Text(
                  review.authorName ?? '',
                  style: theme.textTheme.titleSmall,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
              RatingStars(
                average: (review.rating ?? 0).toDouble(),
                reviewCount: 1,
                size: 13,
                showLabel: false,
              ),
            ],
          ),
          if ((review.comment ?? '').isNotEmpty) ...[
            const SizedBox(height: AppSpacing.xs),
            Text(review.comment!, style: theme.textTheme.bodySmall),
          ],
        ],
      ),
    );
  }
}
