import 'package:flutter/material.dart';

import '../../core/theme/app_colors.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/rating_summary.dart';
import '../../models/recommendation.dart';
import '../../models/taxi_company.dart';
import '../../providers/recommendation_provider.dart';
import '../../providers/review_provider.dart';
import '../../providers/taxi_company_provider.dart';
import '../../services/city_locator.dart';
import '../../utils/api_client_exception.dart';
import '../../widgets/api_image.dart';
import '../../widgets/rating_stars.dart';
import 'company_detail_screen.dart';

class RecommendedScreen extends StatefulWidget {
  const RecommendedScreen({super.key});

  @override
  State<RecommendedScreen> createState() => _RecommendedScreenState();
}

class _RecommendedScreenState extends State<RecommendedScreen> {
  final _recommendations = RecommendationProvider();
  final _companies = TaxiCompanyProvider();
  final _reviews = ReviewProvider();

  List<Recommendation> _items = const [];
  Map<int, TaxiCompany> _companyById = const {};
  Map<int, RatingSummary> _ratings = const {};
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
      final city = await CityLocator.resolveDefault();

      final result = await _recommendations.get(
        filter: {
          'Page': 1,
          'PageSize': 5,
          if (city?.id != null) 'CityId': city!.id,
        },
      );

      final companies = await _companies.get(
        filter: {
          'IsActive': true,
          'Page': 1,
          'PageSize': 100,
          'SortBy': 'Name',
          if (city?.id != null) 'CityId': city!.id,
        },
      );

      final averages = await _reviews.readCompanyAverages();

      if (!mounted) return;

      setState(() {
        _items = result.items ?? const <Recommendation>[];
        _companyById = {
          for (final c in companies.items ?? const <TaxiCompany>[])
            if (c.id != null) c.id!: c,
        };
        _ratings = {
          for (final a in averages)
            if (a.id != null) a.id!: a,
        };
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

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Center(
        child: SizedBox(
          height: 28,
          width: 28,
          child: CircularProgressIndicator(strokeWidth: 2.5),
        ),
      );
    }

    if (_error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Text(
            _error!,
            textAlign: TextAlign.center,
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
        ),
      );
    }

    final theme = Theme.of(context);

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(AppSpacing.screenPadding),
        children: [
          Text('Picked for you', style: theme.textTheme.headlineSmall),
          const SizedBox(height: AppSpacing.xs),
          Text(
            'Ranked from the rides you have taken and how other passengers rate '
            'each company. Every card says why it is here.',
            style: theme.textTheme.bodySmall,
          ),
          const SizedBox(height: AppSpacing.md),
          if (_items.isEmpty)
            Text(
              'No companies to recommend yet.',
              style: theme.textTheme.bodySmall,
            ),
          for (var i = 0; i < _items.length; i++)
            Padding(
              padding: const EdgeInsets.only(bottom: AppSpacing.sm),
              child: _RecommendationCard(
                rank: i + 1,
                recommendation: _items[i],
                company: _companyById[_items[i].taxiCompanyId ?? -1],
                rating: _ratings[_items[i].taxiCompanyId ?? -1],
              ),
            ),
        ],
      ),
    );
  }
}

class _RecommendationCard extends StatelessWidget {
  const _RecommendationCard({
    required this.rank,
    required this.recommendation,
    required this.company,
    required this.rating,
  });

  final int rank;
  final Recommendation recommendation;
  final TaxiCompany? company;
  final RatingSummary? rating;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final companyId = recommendation.taxiCompanyId;
    final isPersonal = recommendation.isPersonal;

    return Material(
      color: AppColors.surface,
      borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
      child: InkWell(
        onTap: companyId == null
            ? null
            : () => CompanyDetailScreen.open(context, companyId),
        borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
        child: Container(
          padding: const EdgeInsets.all(AppSpacing.md),
          decoration: BoxDecoration(
            border: Border.all(color: AppColors.border),
            borderRadius: BorderRadius.circular(AppSpacing.cardRadius),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  _RankBadge(rank: rank),
                  const SizedBox(width: AppSpacing.sm),
                  ApiImage.logo(url: company?.logoImageUrl, size: 48),
                  const SizedBox(width: AppSpacing.md),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          recommendation.companyName ?? '',
                          style: theme.textTheme.titleMedium,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                        if ((recommendation.cityName ?? '').isNotEmpty)
                          Text(
                            recommendation.cityName!,
                            style: theme.textTheme.bodySmall,
                          ),
                        const SizedBox(height: AppSpacing.xs),
                        RatingStars(
                          average: rating?.averageRating,
                          reviewCount: rating?.reviewCount,
                        ),
                      ],
                    ),
                  ),
                  const Icon(Icons.chevron_right, color: AppColors.textMuted),
                ],
              ),
              const SizedBox(height: AppSpacing.md),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(AppSpacing.md),
                decoration: BoxDecoration(
                  color: isPersonal
                      ? AppColors.accentCyan.withValues(alpha: 0.1)
                      : AppColors.sectionTint,
                  borderRadius: BorderRadius.circular(AppSpacing.controlRadius),
                ),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Icon(
                      isPersonal ? Icons.history : Icons.groups_outlined,
                      size: 18,
                      color: isPersonal
                          ? AppColors.accentCyan
                          : AppColors.mobileHeader,
                    ),
                    const SizedBox(width: AppSpacing.sm),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            isPersonal ? 'Why you see this' : 'Why this is here',
                            style: TextStyle(
                              fontSize: 11,
                              fontWeight: FontWeight.w700,
                              letterSpacing: 0.5,
                              color: isPersonal
                                  ? AppColors.accentCyan
                                  : AppColors.mobileHeader,
                            ),
                          ),
                          const SizedBox(height: 3),
                          Text(
                            recommendation.explanation ?? '',
                            style: theme.textTheme.bodyMedium,
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _RankBadge extends StatelessWidget {
  const _RankBadge({required this.rank});

  final int rank;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 26,
      height: 26,
      alignment: Alignment.center,
      decoration: const BoxDecoration(
        color: AppColors.mobileHeader,
        shape: BoxShape.circle,
      ),
      child: Text(
        '$rank',
        style: const TextStyle(
          color: Colors.white,
          fontSize: 13,
          fontWeight: FontWeight.w700,
        ),
      ),
    );
  }
}
