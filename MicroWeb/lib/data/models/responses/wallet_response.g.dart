// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'wallet_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

WalletResponse _$WalletResponseFromJson(Map<String, dynamic> json) =>
    $checkedCreate('WalletResponse', json, ($checkedConvert) {
      final val = WalletResponse(
        walletId: $checkedConvert('walletId', (v) => v as String?),
        userId: $checkedConvert('userId', (v) => v as String?),
        balances: $checkedConvert(
          'balances',
          (v) => (v as List<dynamic>?)
              ?.map(
                (e) => WalletBalanceItem.fromJson(e as Map<String, dynamic>),
              )
              .toList(),
        ),
      );
      return val;
    });

Map<String, dynamic> _$WalletResponseToJson(WalletResponse instance) =>
    <String, dynamic>{
      'walletId': ?instance.walletId,
      'userId': ?instance.userId,
      'balances': ?instance.balances?.map((e) => e.toJson()).toList(),
    };

WalletBalanceItem _$WalletBalanceItemFromJson(Map<String, dynamic> json) =>
    $checkedCreate('WalletBalanceItem', json, ($checkedConvert) {
      final val = WalletBalanceItem(
        currency: $checkedConvert(
          'currency',
          (v) => $enumDecodeNullable(_$CurrencyEnumMap, v),
        ),
        balanceMinor: $checkedConvert(
          'balanceMinor',
          (v) => (v as num?)?.toInt(),
        ),
      );
      return val;
    });

Map<String, dynamic> _$WalletBalanceItemToJson(WalletBalanceItem instance) =>
    <String, dynamic>{
      'currency': ?_$CurrencyEnumMap[instance.currency],
      'balanceMinor': ?instance.balanceMinor,
    };

const _$CurrencyEnumMap = {
  Currency.EUR: 0,
  Currency.USD: 1,
  Currency.PLN: 2,
  Currency.GBP: 3,
};
