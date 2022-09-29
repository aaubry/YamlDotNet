use YAML::PP qw/ Load Dump /;

my $input;
while(<>) {
   $input .= $_;
}

my $doc = Load($input);
print(Dump($doc));
