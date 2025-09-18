import 'package:json_annotation/json_annotation.dart';

import '../currency.dart';

part 'wallet_response.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class WalletResponse {
  WalletResponse({this.walletId, this.userId, this.balances});

  @JsonKey(name: r'walletId', required: false, includeIfNull: false)
  final String? walletId;

  @JsonKey(name: r'userId', required: false, includeIfNull: false)
  final String? userId;

  @JsonKey(name: r'balances', required: false, includeIfNull: false)
  final List<WalletBalanceItem>? balances;

  factory WalletResponse.fromJson(Map<String, dynamic> json) => _$WalletResponseFromJson(json);
  Map<String, dynamic> toJson() => _$WalletResponseToJson(this);
}

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class WalletBalanceItem {
  WalletBalanceItem({this.currency, this.balanceMinor});

  @JsonKey(name: r'currency', required: false, includeIfNull: false)
  final Currency? currency;

  @JsonKey(name: r'balanceMinor', required: false, includeIfNull: false)
  final int? balanceMinor;

  factory WalletBalanceItem.fromJson(Map<String, dynamic> json) {
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
    return _$WalletBalanceItemFromJson(data);
  }
  Map<String, dynamic> toJson() => _$WalletBalanceItemToJson(this);
}
