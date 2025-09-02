// CardOptions flags matching backend [Flags] enum values.
// Provides helpers to convert between bitmask integer and Set<CardOptions>.

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

enum CardOptions {
  @JsonValue(0)
  None(0),
  @JsonValue(1)
  ATM(1),
  @JsonValue(2)
  MagneticStripeReader(2),
  @JsonValue(4)
  Contactless(4),
  @JsonValue(8)
  OnlinePayments(8),
  @JsonValue(16)
  AllowChangingSettings(16),
  @JsonValue(32)
  AllowPlasticOrder(32);

  const CardOptions(this.bit);

  final int bit;

  bool get isPowerOfTwo => bit != 0 && (bit & (bit - 1)) == 0;
}

extension CardOptionsFlagSet on Set<CardOptions> {
  int toBitMask() => fold(0, (acc, e) => acc | e.bit);
}

class CardOptionsConverter {
  static Set<CardOptions> fromBitMask(int? mask) {
    if (mask == null) return <CardOptions>{};
    final set = <CardOptions>{};
    for (final opt in CardOptions.values) {
      if (opt.bit == 0) continue; // skip None in combinations
      if ((mask & opt.bit) == opt.bit) set.add(opt);
    }
    if (mask == 0) set.add(CardOptions.None);
    return set;
  }

  static int toBitMask(Set<CardOptions>? set) {
    if (set == null || set.isEmpty) return 0;
    return set.toBitMask();
  }
}
