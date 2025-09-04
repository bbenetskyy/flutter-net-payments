//in c#
//public record PaymentDto(
//     Guid Id,
//     Guid UserId,
//     string BeneficiaryName,
//     string BeneficiaryAccount,
//     Guid? BeneficiaryId,
//     Guid? BeneficiaryAccountId,
//     string FromAccount,
//     decimal Amount,
//     Currency Currency,
//     Currency FromCurrency,
//     string? Details,
//     PaymentStatus Status,
//     DateTime CreatedAt,
//     DateTime? UpdatedAt
// );

import 'package:json_annotation/json_annotation.dart';

part 'payment_response.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class PaymentResponse {
  /// Returns a new [PaymentResponse] instance.
  PaymentResponse({
    required this.id,
    required this.userId,
    required this.beneficiaryName,
    required this.beneficiaryAccount,
    this.beneficiaryId,
    this.beneficiaryAccountId,
    required this.fromAccount,
    required this.amount,
    required this.currency,
    required this.fromCurrency,
    this.details,
    required this.status,
    required this.createdAt,
    this.updatedAt,
  });

  @JsonKey(name: r'id', required: true, includeIfNull: false)
  final String id;
  @JsonKey(name: r'userId', required: true, includeIfNull: false)
  final String userId;
  @JsonKey(name: r'beneficiaryName', required: true, includeIfNull: false)
  final String beneficiaryName;
  @JsonKey(name: r'beneficiaryAccount', required: true, includeIfNull: false)
  final String beneficiaryAccount;
  @JsonKey(name: r'beneficiaryId', required: false, includeIfNull: false)
  final String? beneficiaryId;
  @JsonKey(name: r'beneficiaryAccountId', required: false, includeIfNull: false)
  final String? beneficiaryAccountId;
  @JsonKey(name: r'fromAccount', required: true, includeIfNull: false)
  final String fromAccount;
  @JsonKey(name: r'amount', required: true, includeIfNull: false)
  final double amount;
  @JsonKey(name: r'currency', required: true, includeIfNull: false)
  final String currency;
  @JsonKey(name: r'fromCurrency', required: true, includeIfNull: false)
  final String fromCurrency;
  @JsonKey(name: r'details', required: false, includeIfNull: false)
  final String? details;
  @JsonKey(name: r'status', required: true, includeIfNull: false)
  final String status;
  @JsonKey(name: r'createdAt', required: true, includeIfNull: false)
  final DateTime createdAt;
  @JsonKey(name: r'updatedAt', required: false, includeIfNull: false)
  final DateTime? updatedAt;
  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is PaymentResponse &&
          other.id == id &&
          other.userId == userId &&
          other.beneficiaryName == beneficiaryName &&
          other.beneficiaryAccount == beneficiaryAccount &&
          other.beneficiaryId == beneficiaryId &&
          other.beneficiaryAccountId == beneficiaryAccountId &&
          other.fromAccount == fromAccount &&
          other.amount == amount &&
          other.currency == currency &&
          other.fromCurrency == fromCurrency &&
          other.details == details &&
          other.status == status &&
          other.createdAt == createdAt &&
          other.updatedAt == updatedAt;
  @override
  int get hashCode =>
      id.hashCode +
      userId.hashCode +
      beneficiaryName.hashCode +
      beneficiaryAccount.hashCode +
      (beneficiaryId == null ? 0 : beneficiaryId.hashCode) +
      (beneficiaryAccountId == null ? 0 : beneficiaryAccountId.hashCode) +
      fromAccount.hashCode +
      amount.hashCode +
      currency.hashCode +
      fromCurrency.hashCode +
      (details == null ? 0 : details.hashCode) +
      status.hashCode +
      createdAt.hashCode +
      (updatedAt == null ? 0 : updatedAt.hashCode);

  factory PaymentResponse.fromJson(Map<String, dynamic> json) => _$PaymentResponseFromJson(json);

  Map<String, dynamic> toJson() => _$PaymentResponseToJson(this);

  static List<PaymentResponse> listFromJson(dynamic data) {
    if (data is List) {
      return data.map((e) => PaymentResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map && data['items'] is List) {
      return (data['items'] as List).map((e) => PaymentResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map && data['data'] is List) {
      return (data['data'] as List).map((e) => PaymentResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map<String, dynamic>) {
      return [PaymentResponse.fromJson(data)];
    }
    return const [];
  }

  @override
  String toString() {
    return toJson().toString();
  }
}
