//in C# its
//public record AccountDto(Guid Id, Guid UserId, string Iban, Common.Domain.Enums.Currency Currency, DateTime CreatedAt);

import 'package:json_annotation/json_annotation.dart';
import '../currency.dart';
part 'account_response.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class AccountResponse {
  /// Returns a new [AccountResponse] instance.
  AccountResponse({this.id, this.userId, this.iban, this.currency, this.createdAt});

  @JsonKey(name: r'id', required: false, includeIfNull: false)
  final String? id;

  @JsonKey(name: r'userId', required: false, includeIfNull: false)
  final String? userId;

  @JsonKey(name: r'iban', required: false, includeIfNull: false)
  final String? iban;

  @JsonKey(name: r'currency', required: false, includeIfNull: false)
  final Currency? currency;

  @JsonKey(name: r'createdAt', required: false, includeIfNull: false)
  final DateTime? createdAt;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is AccountResponse &&
          other.id == id &&
          other.userId == userId &&
          other.iban == iban &&
          other.currency == currency &&
          other.createdAt == createdAt;

  @override
  int get hashCode =>
      (id == null ? 0 : id.hashCode) +
      (userId == null ? 0 : userId.hashCode) +
      (iban == null ? 0 : iban.hashCode) +
      currency.hashCode +
      (createdAt == null ? 0 : createdAt.hashCode);

  factory AccountResponse.fromJson(Map<String, dynamic> json) {
    // Normalize currency value: backend may return ISO strings (e.g., "USD") while
    // the generated mapper expects numeric enum values (0..3). We convert here.
    final data = Map<String, dynamic>.from(json);
    final cur = data['currency'];
    if (cur is String) {
      final lc = cur.toUpperCase();
      switch (lc) {
        case 'EUR':
          data['currency'] = 0;
          break;
        case 'USD':
          data['currency'] = 1;
          break;
        case 'PLN':
          data['currency'] = 2;
          break;
        case 'GBP':
          data['currency'] = 3;
          break;
        default:
          // Also handle numeric strings like '0', '1', etc.
          final asInt = int.tryParse(cur);
          if (asInt != null) {
            data['currency'] = asInt;
          }
      }
    }
    return _$AccountResponseFromJson(data);
  }

  Map<String, dynamic> toJson() => _$AccountResponseToJson(this);

  static List<AccountResponse> listFromJson(dynamic data) {
    if (data is List) {
      return data.map((e) => AccountResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map && data['items'] is List) {
      return (data['items'] as List).map((e) => AccountResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map && data['data'] is List) {
      return (data['data'] as List).map((e) => AccountResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map<String, dynamic>) {
      return [AccountResponse.fromJson(data)];
    }
    return const [];
  }

  @override
  String toString() {
    return toJson().toString();
  }
}
