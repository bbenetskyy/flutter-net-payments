import 'package:json_annotation/json_annotation.dart';
import 'package:micro_web/data/models/currency.dart';
import 'package:micro_web/data/models/ledger_type.dart';

import '../ledger_account.dart';

part 'ledger_response.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class LedgerResponse {
  LedgerResponse({
    this.id,
    this.walletId,
    this.amountMinor,
    this.currency,
    this.type,
    this.account,
    this.counterpartyAccount,
    this.description,
    this.correlationId,
    this.createdAt,
  });

  @JsonKey(name: r'id', required: false, includeIfNull: false)
  final String? id;

  @JsonKey(name: r'walletId', required: false, includeIfNull: false)
  final String? walletId;

  @JsonKey(name: r'amountMinor', required: false, includeIfNull: false)
  final int? amountMinor;

  @JsonKey(name: r'currency', required: false, includeIfNull: false)
  final Currency? currency;

  @JsonKey(name: r'type', required: false, includeIfNull: false)
  final LedgerType? type;

  @JsonKey(name: r'account', required: false, includeIfNull: false)
  final LedgerAccount? account;

  @JsonKey(name: r'counterpartyAccount', required: false, includeIfNull: false)
  final LedgerAccount? counterpartyAccount;

  @JsonKey(name: r'description', required: false, includeIfNull: false)
  final String? description;

  @JsonKey(name: r'correlationId', required: false, includeIfNull: false)
  final String? correlationId;

  @JsonKey(name: r'createdAt', required: false, includeIfNull: false)
  final DateTime? createdAt;

  factory LedgerResponse.fromJson(Map<String, dynamic> json) {
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
          final asInt = int.tryParse(cur);
          if (asInt != null) {
            data['currency'] = asInt;
          }
      }
    }

    // Normalize 'type' to enum integer values if provided as string
    final typ = data['type'];
    if (typ is String) {
      final lt = typ.toLowerCase();
      switch (lt) {
        case 'credit':
          data['type'] = 1;
          break;
        case 'debit':
          data['type'] = 2;
          break;
        default:
          final asInt = int.tryParse(typ);
          if (asInt != null) {
            data['type'] = asInt;
          }
      }
    }

    // Normalize 'account' if provided as string
    final acc = data['account'];
    if (acc is String) {
      final la = acc.toLowerCase();
      switch (la) {
        case 'cash':
          data['account'] = 1;
          break;
        case 'clearing':
          data['account'] = 2;
          break;
        default:
          final asInt = int.tryParse(acc);
          if (asInt != null) {
            data['account'] = asInt;
          }
      }
    }

    // Normalize 'counterpartyAccount' if provided as string
    final cp = data['counterpartyAccount'];
    if (cp is String) {
      final lcp = cp.toLowerCase();
      switch (lcp) {
        case 'cash':
          data['counterpartyAccount'] = 1;
          break;
        case 'clearing':
          data['counterpartyAccount'] = 2;
          break;
        default:
          final asInt = int.tryParse(cp);
          if (asInt != null) {
            data['counterpartyAccount'] = asInt;
          }
      }
    }

    return _$LedgerResponseFromJson(data);
  }
  Map<String, dynamic> toJson() => _$LedgerResponseToJson(this);
}
